using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Reservations;

public interface IReservationService : IEntityService<Reservation, ReservationSearchObject, ReservationSortBy, EntityIncludes>;

/// <summary>
/// Wraps the default repository to validate reservations and to auto-approve/pend new
/// reservations based on whether any selected room requires manual approval.
/// </summary>
public class ReservationManager(
    IEntityRepository<Reservation, ReservationSearchObject, ReservationSortBy, EntityIncludes> service,
    RoomPlannerDbContext dbContext)
    : EntityWrappingServiceBase<Reservation, ReservationSearchObject, ReservationSortBy, EntityIncludes>(service), IReservationService
{
    public override async Task Add(Reservation item, CancellationToken token = default)
    {
        Validate(item);

        var roomIds = item.Rooms?.Select(r => r.RoomId).ToList() ?? [];
        var requiresApproval = await dbContext.MeetingRooms
            .Where(r => roomIds.Contains(r.Id))
            .AnyAsync(r => r.RequiresApproval, token);
        item.Status = requiresApproval ? ReservationStatus.Pending : ReservationStatus.Approved;

        await base.Add(item, token);
    }

    public override Task<Reservation?> Modify(Reservation item, CancellationToken token = default)
    {
        Validate(item);
        return base.Modify(item, token);
    }

    private static void Validate(Reservation item)
    {
        if (item.Rooms is not { Count: > 0 })
        {
            throw new EntityInputException<Reservation>("Saving reservation failed")
            {
                InputErrors = { ["Rooms"] = "Reservation must include at least one room." }
            };
        }
        if (item.EndTime <= item.StartTime)
        {
            throw new EntityInputException<Reservation>("Saving reservation failed")
            {
                InputErrors = { ["EndTime"] = "End time must be after start time." }
            };
        }
    }
}
