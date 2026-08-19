using Regira.Entities.QueryBuilders.Abstractions;

namespace AssetHub.Api.Entities.AssetAssignments;

public class AssetAssignmentQueryBuilder : IFilteredQueryBuilder<AssetAssignment, int, AssetAssignmentSearchObject>
{
    public IQueryable<AssetAssignment> Build(IQueryable<AssetAssignment> query, AssetAssignmentSearchObject? so)
    {
        if (so == null)
        {
            return query;
        }

        if (so.AssetId?.Any() == true)
        {
            query = query.Where(x => so.AssetId.Contains(x.AssetId));
        }
        if (so.EmployeeId?.Any() == true)
        {
            query = query.Where(x => so.EmployeeId.Contains(x.EmployeeId));
        }
        if (so.IsActive != null)
        {
            query = so.IsActive.Value
                ? query.Where(x => x.ReturnedDate == null)
                : query.Where(x => x.ReturnedDate != null);
        }
        if (so.MinAssignedDate != null)
        {
            query = query.Where(x => x.AssignedDate >= so.MinAssignedDate.Value);
        }
        if (so.MaxAssignedDate != null)
        {
            query = query.Where(x => x.AssignedDate <= so.MaxAssignedDate.Value);
        }

        return query;
    }
}
