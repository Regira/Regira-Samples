using Regira.Entities.QueryBuilders.Abstractions;

namespace Fleet.API.Entities.Vehicles;

public class VehicleQueryBuilder : FilteredQueryBuilderBase<Vehicle, int, VehicleSearchObject>
{
    public override IQueryable<Vehicle> Build(IQueryable<Vehicle> query, VehicleSearchObject? so)
    {
        if (so == null) return query;

        if (so.VehicleType?.Any() == true)
            query = query.Where(x => so.VehicleType.Contains(x.VehicleType));
        if (!string.IsNullOrWhiteSpace(so.Brand))
            query = query.Where(x => x.Brand == so.Brand);
        if (so.InterventionTypeId?.Any() == true)
            query = query.Where(x => x.AllowedInterventionTypes!.Any(t => so.InterventionTypeId.Contains(t.InterventionTypeId)));
        if (so.MinYear.HasValue)
            query = query.Where(x => x.Year >= so.MinYear.Value);
        if (so.MaxYear.HasValue)
            query = query.Where(x => x.Year <= so.MaxYear.Value);

        return query;
    }
}
