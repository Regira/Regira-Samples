using AssetHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Processing.Abstractions;

namespace AssetHub.Api.Entities.Employees;

/// <summary>Fills the [NotMapped] CurrentAssetCount with one grouped query per page</summary>
public class EmployeeProcessor(AppDbContext dbContext) : IEntityProcessor<Employee, EmployeeIncludes>
{
    public async Task Process(IList<Employee> items, EmployeeIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Assets.AsNoTracking()
            .Where(x => x.CurrentEmployeeId != null && ids.Contains(x.CurrentEmployeeId.Value))
            .GroupBy(x => x.CurrentEmployeeId!.Value)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, token);
        foreach (var item in items)
            item.CurrentAssetCount = counts.GetValueOrDefault(item.Id);
    }
}
