using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using QCredits.Api.Balances;
using QCredits.Api.Data;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.QCreditRequests;

namespace QCredits.Api.Controllers;

/// <summary>
/// The approve/reject state machine for a QCreditRequest - a domain action beside the entity
/// controller, on the same resource route. Status/DecisionDate/ApproverId/DecisionNotes are kept off
/// QCreditRequestInputDto, so this controller (flipping RequestWorkflowContext.IsTrustedWriter) is the
/// only writer of those fields; QCreditRequestStatusPrimer restores them on every other write.
/// </summary>
[ApiController, Route("qcredit-requests")]
public class QCreditRequestWorkflowController(
    IEntityService<QCreditRequest, QCreditRequestSearchObject, EntitySortBy, QCreditRequestIncludes> service,
    QCreditsDbContext db,
    BalanceCalculator calculator,
    RequestWorkflowContext workflow) : ControllerBase
{
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] DecisionInput input, CancellationToken token)
    {
        var item = await service.Details(id, token);
        if (item == null)
        {
            return NotFound();
        }
        if (item.Status != RequestStatus.Pending)
        {
            ModelState.AddModelError(nameof(item.Status), "Only a pending request can be approved.");
            return BadRequest(ModelState);
        }

        var approver = await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == input.ApproverId, token);
        if (approver == null || approver.Role != EmployeeRole.Admin)
        {
            ModelState.AddModelError(nameof(input.ApproverId), "The approver must be an existing administrator.");
            return BadRequest(ModelState);
        }

        var balance = await calculator.Compute(item.EmployeeId, item.Year, token);
        if (balance == null)
        {
            return NotFound();
        }
        var projectedRemaining = balance.RemainingCredits - item.TotalCredits;
        if (projectedRemaining < balance.MinBalance)
        {
            ModelState.AddModelError(nameof(item.TotalCredits),
                $"Approving this request would bring {balance.EmployeeName}'s balance to {projectedRemaining:0.##} QCredits, below the allowed minimum of {balance.MinBalance:0.##}.");
            return BadRequest(ModelState);
        }

        workflow.IsTrustedWriter = true;
        item.Status = RequestStatus.Approved;
        item.DecisionDate = DateTime.UtcNow;
        item.ApproverId = input.ApproverId;
        item.DecisionNotes = input.Notes;
        await service.Modify(item, token);
        await service.SaveChanges(token);

        return Ok(new { item.Id, item.Status, item.DecisionDate, item.ApproverId, item.DecisionNotes });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] DecisionInput input, CancellationToken token)
    {
        var item = await service.Details(id, token);
        if (item == null)
        {
            return NotFound();
        }
        if (item.Status != RequestStatus.Pending)
        {
            ModelState.AddModelError(nameof(item.Status), "Only a pending request can be rejected.");
            return BadRequest(ModelState);
        }

        var approver = await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == input.ApproverId, token);
        if (approver == null || approver.Role != EmployeeRole.Admin)
        {
            ModelState.AddModelError(nameof(input.ApproverId), "The approver must be an existing administrator.");
            return BadRequest(ModelState);
        }

        workflow.IsTrustedWriter = true;
        item.Status = RequestStatus.Rejected;
        item.DecisionDate = DateTime.UtcNow;
        item.ApproverId = input.ApproverId;
        item.DecisionNotes = input.Notes;
        await service.Modify(item, token);
        await service.SaveChanges(token);

        return Ok(new { item.Id, item.Status, item.DecisionDate, item.ApproverId, item.DecisionNotes });
    }
}
