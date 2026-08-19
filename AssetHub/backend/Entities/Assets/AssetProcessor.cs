using AssetHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Processing.Abstractions;

namespace AssetHub.Api.Entities.Assets;

// Fills the [NotMapped] "current holder" fields from the active (unreturned) assignment, if any.
public class AssetProcessor(AppDbContext dbContext) : IEntityProcessor<Asset, AssetIncludes>
{
    public async Task Process(IList<Asset> items, AssetIncludes? includes, CancellationToken token = default)
    {
        var assetIds = items.Select(x => x.Id).ToList();
        var active = await dbContext.AssetAssignments
            .Where(a => a.ReturnedDate == null && assetIds.Contains(a.AssetId))
            .Select(a => new { a.AssetId, a.EmployeeId, EmployeeName = a.Employee!.FirstName + " " + a.Employee.LastName, a.AssignedDate })
            .ToDictionaryAsync(a => a.AssetId, token);

        foreach (var item in items)
        {
            if (active.TryGetValue(item.Id, out var assignment))
            {
                item.CurrentEmployeeId = assignment.EmployeeId;
                item.CurrentEmployeeName = assignment.EmployeeName;
                item.CurrentAssignedDate = assignment.AssignedDate;
            }
        }
    }
}
