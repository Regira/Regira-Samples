using QCredits.Api.Services;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;

namespace QCredits.Api.Entities.CreditAllocations;

/// <summary>Fills the computed balance fields (free / used / pending / remaining) after fetching.</summary>
public class CreditAllocationProcessor(BalanceService balances) : IEntityProcessor<CreditAllocation, EntityIncludes>
{
    public async Task Process(IList<CreditAllocation> items, EntityIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var employeeIds = items.Select(x => x.EmployeeId).Distinct().ToList();
        var years = items.Select(x => x.Year).Distinct().ToList();
        var usage = await balances.GetUsage(employeeIds, years, token);
        foreach (var item in items)
            balances.Apply(item, usage.GetValueOrDefault((item.EmployeeId, item.Year)));
    }
}
