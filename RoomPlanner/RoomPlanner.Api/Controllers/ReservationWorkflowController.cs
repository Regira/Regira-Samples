using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.Controllers;
using Regira.Entities.Web.Models;
using RoomPlanner.Api.Entities.Reservations;
using RoomPlanner.Api.Services;

namespace RoomPlanner.Api.Controllers;

/// <summary>
/// Domain actions on the reservation resource: per-room approval and cancellation.
/// These are the only writers of the approval / cancel state (see ReservationPrepper).
/// </summary>
[ApiController, Route("reservations")]
public class ReservationWorkflowController(IEntityService<Reservation, int> service, WorkflowContext workflow) : ControllerBase
{
    [HttpPost("{id:int}/rooms/{roomId:int}/approve")]
    public Task<ActionResult<DetailsResult<ReservationDto>>> Approve(int id, int roomId, [FromBody] DecisionInput? input)
        => Decide(id, roomId, RoomApprovalStatus.Approved, input?.Note);

    [HttpPost("{id:int}/rooms/{roomId:int}/reject")]
    public Task<ActionResult<DetailsResult<ReservationDto>>> Reject(int id, int roomId, [FromBody] DecisionInput? input)
        => Decide(id, roomId, RoomApprovalStatus.Rejected, input?.Note);

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<DetailsResult<ReservationDto>>> Cancel(int id, [FromBody] DecisionInput? input)
    {
        var item = await service.Details(id);
        if (item == null) return NotFound();
        if (item.Status == ReservationStatus.Cancelled)
            throw Invalid(nameof(Reservation.Status), "The reservation is already cancelled.");

        workflow.IsTrustedWriter = true;
        item.Status = ReservationStatus.Cancelled;
        item.CancelledOn = DateTime.UtcNow;
        item.CancelReason = input?.Note;
        await service.Modify(item);
        await service.SaveChanges();
        return await this.Details<Reservation, ReservationDto>(id) ?? NotFound();
    }

    async Task<ActionResult<DetailsResult<ReservationDto>>> Decide(int id, int roomId, RoomApprovalStatus decision, string? note)
    {
        var item = await service.Details(id);
        if (item == null) return NotFound();
        if (item.Status == ReservationStatus.Cancelled)
            throw Invalid(nameof(Reservation.Status), "A cancelled reservation can't be approved or rejected.");
        var row = item.Rooms?.FirstOrDefault(x => x.RoomId == roomId);
        if (row == null) return NotFound();
        if (row.ApprovalStatus == decision)
            throw Invalid(nameof(Reservation.Rooms), $"This room is already {decision.ToString().ToLowerInvariant()}.");

        workflow.IsTrustedWriter = true;
        row.ApprovalStatus = decision;
        row.DecidedOn = DateTime.UtcNow;
        row.DecisionNote = note;
        await service.Modify(item);   // the prepper re-derives the reservation status and re-checks conflicts
        await service.SaveChanges();
        return await this.Details<Reservation, ReservationDto>(id) ?? NotFound();
    }

    static EntityInputException<Reservation> Invalid(string key, string message)
        => new(message) { InputErrors = { [key] = message } };
}
