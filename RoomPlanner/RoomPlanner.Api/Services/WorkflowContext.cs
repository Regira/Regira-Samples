namespace RoomPlanner.Api.Services;

/// <summary>
/// Scoped flag: set by the reservation workflow actions (approve / reject / cancel) and the seeder,
/// so <see cref="Entities.Reservations.ReservationPrepper"/> accepts the approval / cancel state they write.
/// Every other write path gets those fields restored from the stored row.
/// </summary>
public sealed class WorkflowContext
{
    public bool IsTrustedWriter { get; set; }
}
