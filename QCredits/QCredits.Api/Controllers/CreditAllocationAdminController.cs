using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using QCredits.Api.Entities.CreditAllocations;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Entities.CreditYears;
using QCredits.Api.Infrastructure.Security;
using QCredits.Api.Services;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace QCredits.Api.Controllers;

public class YearInput
{
    public int Year { get; set; }
}

public record AllocationBatchResult(int Year, int Created, int Updated, string Message);

/// <summary>Administrator actions on yearly allocations: generate a year, roll balances over to the next year.</summary>
[ApiController, Route("credit-allocations"), Authorize(Roles = Roles.Admin)]
public class CreditAllocationAdminController(
    IEntityService<CreditAllocation, int> allocations,
    IEntityService<CreditYear, int> years,
    WorkflowContext workflow,
    BalanceService balances,
    AppDbContext db) : ControllerBase
{
    /// <summary>Creates the missing allocations of a year for every active employee, using that year's policy.</summary>
    [HttpPost("generate")]
    public async Task<ActionResult<AllocationBatchResult>> Generate([FromBody] YearInput input)
    {
        var policy = await db.CreditYears.AsNoTracking().FirstOrDefaultAsync(x => x.Year == input.Year)
            ?? throw Invalid(nameof(YearInput.Year), $"Create the credit policy for {input.Year} first.");

        var existing = await db.CreditAllocations.Where(x => x.Year == input.Year).Select(x => x.EmployeeId).ToListAsync();
        var employeeIds = await db.Employees.Where(x => x.IsActive && !existing.Contains(x.Id)).Select(x => x.Id).ToListAsync();

        workflow.IsTrustedWriter = true;
        foreach (var employeeId in employeeIds)
            await allocations.Add(NewAllocation(employeeId, policy, 0));
        await allocations.SaveChanges();
        workflow.IsTrustedWriter = false;

        return new AllocationBatchResult(input.Year, employeeIds.Count, 0, $"{employeeIds.Count} allocation(s) created for {input.Year}.");
    }

    /// <summary>
    /// Closes a year and carries each remaining balance over to the next year:
    /// positive balances up to the policy's MaxCarryOver, a deficit (negative balance) is carried over in full
    /// (it can never be lower than the minimum balance). Refused while requests of that year are still pending.
    /// </summary>
    [HttpPost("rollover")]
    public async Task<ActionResult<AllocationBatchResult>> Rollover([FromBody] YearInput input)
    {
        var fromYear = input.Year;
        var toYear = fromYear + 1;
        var policy = await db.CreditYears.AsNoTracking().FirstOrDefaultAsync(x => x.Year == fromYear)
            ?? throw Invalid(nameof(YearInput.Year), $"There is no credit policy for {fromYear}.");
        var pending = await db.CreditRequests.CountAsync(x => x.Year == fromYear && x.Status == RequestStatus.Submitted);
        if (pending > 0)
            throw Invalid(nameof(YearInput.Year), $"{pending} request(s) of {fromYear} are still waiting for approval. Decide on them first.");

        workflow.IsTrustedWriter = true;
        try
        {
            // next year's policy: copy when missing
            var nextPolicy = await db.CreditYears.AsNoTracking().FirstOrDefaultAsync(x => x.Year == toYear);
            if (nextPolicy == null)
            {
                nextPolicy = new CreditYear
                {
                    Year = toYear,
                    AnnualCredits = policy.AnnualCredits,
                    ReservedCredits = policy.ReservedCredits,
                    MaxCarryOver = policy.MaxCarryOver,
                    MinBalance = policy.MinBalance
                };
                await years.Add(nextPolicy);
            }
            var closing = await db.CreditYears.AsNoTracking().FirstAsync(x => x.Year == fromYear);
            closing.IsClosed = true;
            await years.Modify(closing);
            await years.SaveChanges();

            var source = await db.CreditAllocations.AsNoTracking().Where(x => x.Year == fromYear).ToListAsync();
            var usage = await balances.GetUsage(source.Select(x => x.EmployeeId).Distinct().ToList(), [fromYear]);
            var targets = await db.CreditAllocations.AsNoTracking().Where(x => x.Year == toYear)
                .ToDictionaryAsync(x => x.EmployeeId);

            int created = 0, updated = 0;
            foreach (var allocation in source)
            {
                balances.Apply(allocation, usage.GetValueOrDefault((allocation.EmployeeId, fromYear)));
                var carry = CarryOver(allocation.RemainingCredits, policy.MaxCarryOver, allocation.MinBalance);
                if (targets.TryGetValue(allocation.EmployeeId, out var target))
                {
                    target.CarriedOver = carry;
                    await allocations.Modify(target);
                    updated++;
                }
                else
                {
                    await allocations.Add(NewAllocation(allocation.EmployeeId, nextPolicy, carry));
                    created++;
                }
            }
            await allocations.SaveChanges();
            return new AllocationBatchResult(toYear, created, updated,
                $"{fromYear} closed: {created} allocation(s) created and {updated} updated for {toYear}.");
        }
        finally
        {
            workflow.IsTrustedWriter = false;
        }
    }

    public static double CarryOver(double remaining, double maxCarryOver, double minBalance)
        => Math.Max(Math.Min(remaining, maxCarryOver), minBalance);

    private static CreditAllocation NewAllocation(int employeeId, CreditYear policy, double carriedOver) => new()
    {
        EmployeeId = employeeId,
        Year = policy.Year,
        AnnualCredits = policy.AnnualCredits,
        ReservedCredits = policy.ReservedCredits,
        MinBalance = policy.MinBalance,
        CarriedOver = carriedOver
    };

    private static EntityInputException<CreditAllocation> Invalid(string field, string message)
        => new(message) { InputErrors = { [field] = message } };
}
