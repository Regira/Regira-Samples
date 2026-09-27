using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;
using RoomPlanner.Api.Data;
using RoomPlanner.Api.Services;

namespace RoomPlanner.Api.Entities.Reservations;

/// <summary>
/// Validates a reservation (schedule, rooms, conflicts, capacity) and owns its server-side state:
/// the per-room approval (auto-approved or pending, depending on <c>Room.RequiresApproval</c>),
/// the derived <see cref="Reservation.Status"/>, the attendee count and the cancel fields.
/// Only a trusted writer (<see cref="WorkflowContext"/>: workflow actions, seeder) may set approval/cancel state.
/// Registered BEFORE the Related() syncs, so the rows it completes are the rows that get saved.
/// </summary>
public class ReservationPrepper(AppDbContext db, WorkflowContext workflow) : EntityPrepperBase<Reservation>
{
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(24);

    public override async Task Prepare(Reservation modified, Reservation? original, CancellationToken token = default)
    {
        var trusted = workflow.IsTrustedWriter;
        var errors = new Dictionary<string, string>();
        var now = DateTime.UtcNow;

        if (original != null && !trusted)
        {
            if (original.Status == ReservationStatus.Cancelled)
                Throw(new() { [nameof(Reservation.Status)] = "A cancelled reservation can't be modified." });
            // server-owned fields: restore from the stored row
            modified.CancelledOn = original.CancelledOn;
            modified.CancelReason = original.CancelReason;
        }

        // --- schedule ---
        if (modified.End <= modified.Start)
            errors[nameof(Reservation.End)] = "End must be after start.";
        else if (modified.End - modified.Start > MaxDuration)
            errors[nameof(Reservation.End)] = "A reservation can't last longer than 24 hours.";
        var rescheduled = original != null && (original.Start != modified.Start || original.End != modified.End);

        // --- organizer ---
        if (!await db.Employees.AnyAsync(x => x.Id == modified.OrganizerId, token))
            errors[nameof(Reservation.OrganizerId)] = "Unknown organizer.";

        // --- rooms: a PATCH without rooms keeps the stored rows (cloned, so a reschedule can still re-evaluate them) ---
        if (modified.Rooms == null && original?.Rooms != null)
        {
            modified.Rooms = original.Rooms.Select(r => new ReservationRoom
            {
                Id = r.Id,
                ReservationId = r.ReservationId,
                RoomId = r.RoomId,
                ApprovalStatus = r.ApprovalStatus,
                DecidedOn = r.DecidedOn,
                DecisionNote = r.DecisionNote,
            }).ToList();
        }
        var rows = modified.Rooms?.ToList() ?? [];
        if (rows.Count == 0)
            errors[nameof(Reservation.Rooms)] = "Select at least one room.";
        else if (rows.GroupBy(x => x.RoomId).Any(g => g.Count() > 1))
            errors[nameof(Reservation.Rooms)] = "A room can only be added once.";

        var roomIds = rows.Select(x => x.RoomId).Distinct().ToList();
        var rooms = await db.Rooms.AsNoTracking()
            .Where(x => roomIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Title, x.Capacity, x.RequiresApproval, x.IsActive })
            .ToDictionaryAsync(x => x.Id, token);

        foreach (var row in rows)
        {
            if (!rooms.TryGetValue(row.RoomId, out var room))
            {
                errors[nameof(Reservation.Rooms)] = $"Unknown room #{row.RoomId}.";
                continue;
            }
            var stored = original?.Rooms?.FirstOrDefault(x => x.RoomId == row.RoomId);
            if (stored == null && !room.IsActive && !trusted)
                errors[nameof(Reservation.Rooms)] = $"{room.Title} is out of service.";

            if (trusted) continue; // workflow action / seeder decides the approval state itself

            if (stored == null)
            {
                // new booking of this room: auto-approve unless the room requires approval
                row.ApprovalStatus = room.RequiresApproval ? RoomApprovalStatus.Pending : RoomApprovalStatus.Approved;
                row.DecidedOn = room.RequiresApproval ? null : now;
                row.DecisionNote = room.RequiresApproval ? null : "Automatically approved";
            }
            else
            {
                row.ApprovalStatus = stored.ApprovalStatus;
                row.DecidedOn = stored.DecidedOn;
                row.DecisionNote = stored.DecisionNote;
                if (rescheduled && room.RequiresApproval && row.ApprovalStatus != RoomApprovalStatus.Pending)
                {
                    row.ApprovalStatus = RoomApprovalStatus.Pending;
                    row.DecidedOn = null;
                    row.DecisionNote = "Rescheduled - approval required again";
                }
                else if (rescheduled && !room.RequiresApproval)
                {
                    row.ApprovalStatus = RoomApprovalStatus.Approved;
                    row.DecidedOn = now;
                    row.DecisionNote = "Automatically approved";
                }
            }
        }

        // --- attendees ---
        if (modified.Attendees != null)
        {
            if (modified.Attendees.GroupBy(x => x.EmployeeId).Any(g => g.Count() > 1))
                errors[nameof(Reservation.Attendees)] = "An attendee can only be invited once.";
            var employeeIds = modified.Attendees.Select(x => x.EmployeeId).Distinct().ToList();
            var known = await db.Employees.CountAsync(x => employeeIds.Contains(x.Id), token);
            if (known != employeeIds.Count)
                errors[nameof(Reservation.Attendees)] = "Unknown attendee.";
            modified.AttendeeCount = modified.Attendees.Count;
        }
        else
        {
            modified.AttendeeCount = original?.AttendeeCount ?? 0;
        }

        // --- status (derived from the room rows, unless cancelled) ---
        var cancelled = trusted ? modified.Status == ReservationStatus.Cancelled : original?.Status == ReservationStatus.Cancelled;
        modified.Status = cancelled ? ReservationStatus.Cancelled : DeriveStatus(rows);

        // --- capacity & conflicts only matter for a live booking ---
        if (!cancelled && errors.Count == 0)
        {
            // capacity is checked against every requested room: a later rejection is the facility's decision
            // and must not make the reservation itself invalid (nor block the reject action)
            var liveRows = rows.Where(x => x.ApprovalStatus != RoomApprovalStatus.Rejected).ToList();
            var capacity = rows.Sum(x => rooms[x.RoomId].Capacity);
            if (!trusted && rows.Count > 0 && modified.AttendeeCount + 1 > capacity)
                errors[nameof(Reservation.Attendees)] = $"{modified.AttendeeCount + 1} people exceed the capacity of the selected room(s) ({capacity}).";

            var conflict = await FindConflict(modified, liveRows.Select(x => x.RoomId).ToList(), token);
            if (conflict != null)
                errors[nameof(Reservation.Rooms)] = $"{rooms[conflict.Value.RoomId].Title} is already booked at that time (\"{conflict.Value.Title}\"). Pick another slot or room.";
        }

        if (errors.Count > 0) Throw(errors);
    }

    public static ReservationStatus DeriveStatus(IReadOnlyCollection<ReservationRoom> rows)
    {
        if (rows.Count == 0 || rows.Any(x => x.ApprovalStatus == RoomApprovalStatus.Pending)) return ReservationStatus.Pending;
        if (rows.All(x => x.ApprovalStatus == RoomApprovalStatus.Approved)) return ReservationStatus.Approved;
        if (rows.All(x => x.ApprovalStatus == RoomApprovalStatus.Rejected)) return ReservationStatus.Rejected;
        return ReservationStatus.PartiallyApproved;
    }

    async Task<(int RoomId, string Title, DateTime Start, DateTime End)?> FindConflict(Reservation item, List<int> roomIds, CancellationToken token)
    {
        if (roomIds.Count == 0) return null;
        var persisted = await db.ReservationRooms.AsNoTracking()
            .Where(rr => roomIds.Contains(rr.RoomId)
                         && rr.ReservationId != item.Id
                         && rr.ApprovalStatus != RoomApprovalStatus.Rejected
                         && rr.Reservation!.Status != ReservationStatus.Cancelled
                         && rr.Reservation.Start < item.End
                         && rr.Reservation.End > item.Start)
            .Select(rr => new { rr.RoomId, rr.Reservation!.Title, rr.Reservation.Start, rr.Reservation.End })
            .FirstOrDefaultAsync(token);
        if (persisted != null) return (persisted.RoomId, persisted.Title, persisted.Start, persisted.End);

        // rows queued in the same SaveChanges() batch (bulk writes / seeding) are invisible to the query above
        foreach (var entry in db.ChangeTracker.Entries<Reservation>().Where(e => e.State == EntityState.Added))
        {
            var other = entry.Entity;
            if (ReferenceEquals(other, item) || other.Status == ReservationStatus.Cancelled) continue;
            if (!(other.Start < item.End && other.End > item.Start)) continue;
            var hit = other.Rooms?.FirstOrDefault(r => r.ApprovalStatus != RoomApprovalStatus.Rejected && roomIds.Contains(r.RoomId));
            if (hit != null) return (hit.RoomId, other.Title, other.Start, other.End);
        }
        return null;
    }

    static void Throw(Dictionary<string, string> errors)
    {
        var ex = new EntityInputException<Reservation>("Invalid reservation");
        foreach (var (key, value) in errors) ex.InputErrors[key] = value;
        throw ex;
    }
}
