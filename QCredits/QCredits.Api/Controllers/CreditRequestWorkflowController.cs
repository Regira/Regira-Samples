using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Infrastructure.Security;
using QCredits.Api.Services;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.Controllers;
using Regira.Entities.Web.Models;

namespace QCredits.Api.Controllers;

public class DecisionInput
{
    public string? Comment { get; set; }
}

/// <summary>
/// State transitions of a credit request (same resource route as the CRUD controller).
/// Draft -> Submitted -> Approved | Rejected; Draft/Submitted -> Cancelled; Submitted -> Draft (withdraw);
/// an administrator may also cancel an approved request, which gives the credits back.
/// </summary>
[ApiController, Route("credit-requests")]
public class CreditRequestWorkflowController(
    IEntityService<CreditRequest, int> service,
    WorkflowContext workflow,
    AccessScope scope,
    BalanceService balances,
    AppDbContext db) : ControllerBase
{
    [HttpPost("{id:int}/submit")]
    public Task<ActionResult<DetailsResult<CreditRequestDto>>> Submit(int id)
        => Transition(id, [RequestStatus.Draft], RequestStatus.Submitted, async item =>
        {
            if (item.Items == null || item.Items.Count == 0)
                throw Invalid(nameof(CreditRequest.Items), "Add at least one purchase or activity before submitting.");
            await EnsureOpenYear(item.Year);
            var balance = await balances.GetBalance(item.EmployeeId, item.Year)
                ?? throw Invalid(nameof(CreditRequest.Year), $"No credit allocation exists for {item.Year}.");
            if (item.TotalCredits > balance.Requestable)
                throw Invalid(nameof(CreditRequest.TotalCredits),
                    $"This request needs {item.TotalCredits:0.#} credits, but only {balance.Requestable:0.#} can still be requested (pending requests included, minimum balance {balance.MinBalance:0.#}).");
            item.SubmittedAt = DateTime.UtcNow;
        });

    [HttpPost("{id:int}/withdraw")]
    public Task<ActionResult<DetailsResult<CreditRequestDto>>> Withdraw(int id)
        => Transition(id, [RequestStatus.Submitted], RequestStatus.Draft, item =>
        {
            item.SubmittedAt = null;
            return Task.CompletedTask;
        });

    [HttpPost("{id:int}/approve"), Authorize(Roles = Roles.Admin)]
    public Task<ActionResult<DetailsResult<CreditRequestDto>>> Approve(int id, [FromBody] DecisionInput? input)
        => Transition(id, [RequestStatus.Submitted], RequestStatus.Approved, async item =>
        {
            var balance = await balances.GetBalance(item.EmployeeId, item.Year)
                ?? throw Invalid(nameof(CreditRequest.Year), $"No credit allocation exists for {item.Year}.");
            var after = balance.RemainingCredits - item.TotalCredits;
            if (after < balance.MinBalance)
                throw Invalid(nameof(CreditRequest.TotalCredits),
                    $"Approving would bring the balance to {after:0.#}, below the minimum of {balance.MinBalance:0.#}.");
            Decide(item, input?.Comment);
        });

    [HttpPost("{id:int}/reject"), Authorize(Roles = Roles.Admin)]
    public Task<ActionResult<DetailsResult<CreditRequestDto>>> Reject(int id, [FromBody] DecisionInput? input)
        => Transition(id, [RequestStatus.Submitted], RequestStatus.Rejected, item =>
        {
            if (string.IsNullOrWhiteSpace(input?.Comment))
                throw Invalid(nameof(DecisionInput.Comment), "Please explain why the request is rejected.");
            Decide(item, input.Comment);
            return Task.CompletedTask;
        });

    [HttpPost("{id:int}/cancel")]
    public Task<ActionResult<DetailsResult<CreditRequestDto>>> Cancel(int id, [FromBody] DecisionInput? input)
    {
        RequestStatus[] from = scope.IsAdmin
            ? [RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Approved]
            : [RequestStatus.Draft, RequestStatus.Submitted];
        return Transition(id, from, RequestStatus.Cancelled, item =>
        {
            if (item.Status == RequestStatus.Approved) Decide(item, input?.Comment ?? "Cancelled after approval");
            return Task.CompletedTask;
        });
    }

    private void Decide(CreditRequest item, string? comment)
    {
        item.DecidedAt = DateTime.UtcNow;
        item.DecidedBy = scope.UserName;
        item.DecisionComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    private async Task<ActionResult<DetailsResult<CreditRequestDto>>> Transition(
        int id, RequestStatus[] from, RequestStatus to, Func<CreditRequest, Task> apply)
    {
        // read through the service first: the row filter decides whether the caller may see this request
        var item = await service.Details(id);
        if (item == null) return NotFound();
        if (!from.Contains(item.Status))
            throw Invalid(nameof(CreditRequest.Status), $"A {item.Status} request cannot be changed to {to}.");

        await apply(item);
        item.Status = to;

        workflow.IsTrustedWriter = true;
        try
        {
            await service.Modify(item);
            await service.SaveChanges();
        }
        finally
        {
            workflow.IsTrustedWriter = false;
        }
        return await this.Details<CreditRequest, CreditRequestDto>(id) ?? NotFound();
    }

    private async Task EnsureOpenYear(int year)
    {
        var policy = await db.CreditYears.AsNoTracking().FirstOrDefaultAsync(x => x.Year == year);
        if (policy == null) throw Invalid(nameof(CreditRequest.Year), $"There is no credit policy for {year}.");
        if (policy.IsClosed) throw Invalid(nameof(CreditRequest.Year), $"Credit year {year} is closed.");
    }

    private static EntityInputException<CreditRequest> Invalid(string field, string message)
        => new(message) { InputErrors = { [field] = message } };
}
