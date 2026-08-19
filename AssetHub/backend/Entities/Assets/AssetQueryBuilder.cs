using AssetHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.QueryBuilders.Abstractions;

namespace AssetHub.Api.Entities.Assets;

public class AssetQueryBuilder(AppDbContext dbContext) : IFilteredQueryBuilder<Asset, int, AssetSearchObject>
{
    public IQueryable<Asset> Build(IQueryable<Asset> query, AssetSearchObject? so)
    {
        if (so == null)
        {
            return query;
        }

        if (so.CategoryId?.Any() == true)
        {
            query = query.Where(x => so.CategoryId.Contains(x.CategoryId));
        }
        if (so.StatusId?.Any() == true)
        {
            query = query.Where(x => so.StatusId.Contains(x.StatusId));
        }
        if (so.LocationId?.Any() == true)
        {
            query = query.Where(x => x.LocationId != null && so.LocationId.Contains(x.LocationId.Value));
        }
        if (so.SupplierId?.Any() == true)
        {
            query = query.Where(x => x.SupplierId != null && so.SupplierId.Contains(x.SupplierId.Value));
        }
        if (so.IsAssigned != null)
        {
            var assignedAssetIds = dbContext.AssetAssignments
                .Where(a => a.ReturnedDate == null)
                .Select(a => a.AssetId);
            query = so.IsAssigned.Value
                ? query.Where(x => assignedAssetIds.Contains(x.Id))
                : query.Where(x => !assignedAssetIds.Contains(x.Id));
        }
        if (so.AssignedToEmployeeId != null)
        {
            var employeeId = so.AssignedToEmployeeId.Value;
            var assetIdsForEmployee = dbContext.AssetAssignments
                .Where(a => a.ReturnedDate == null && a.EmployeeId == employeeId)
                .Select(a => a.AssetId);
            query = query.Where(x => assetIdsForEmployee.Contains(x.Id));
        }

        return query;
    }
}
