using Regira.Entities.QueryBuilders.Abstractions;

namespace Fleet.API.Entities.Interventions;

public class InterventionQueryBuilder : FilteredQueryBuilderBase<Intervention, int, InterventionSearchObject>
{
    public override IQueryable<Intervention> Build(IQueryable<Intervention> query, InterventionSearchObject? so)
    {
        if (so == null) return query;

        if (so.VehicleId?.Any() == true)
            query = query.Where(x => so.VehicleId.Contains(x.VehicleId));
        if (so.SupplierId?.Any() == true)
            query = query.Where(x => so.SupplierId.Contains(x.SupplierId));
        if (so.InterventionTypeId?.Any() == true)
            query = query.Where(x => so.InterventionTypeId.Contains(x.InterventionTypeId));
        if (so.InvoiceId?.Any() == true)
            query = query.Where(x => x.InvoiceId != null && so.InvoiceId.Contains(x.InvoiceId.Value));
        if (so.Status?.Any() == true)
            query = query.Where(x => so.Status.Contains(x.Status));
        if (so.IsInvoiced.HasValue)
            query = so.IsInvoiced.Value
                ? query.Where(x => x.InvoiceId != null)
                : query.Where(x => x.InvoiceId == null);
        if (so.MinScheduledDate.HasValue)
            query = query.Where(x => x.ScheduledDate >= so.MinScheduledDate.Value);
        if (so.MaxScheduledDate.HasValue)
            query = query.Where(x => x.ScheduledDate <= so.MaxScheduledDate.Value);
        if (so.MinCost.HasValue)
            query = query.Where(x => x.Cost >= so.MinCost.Value);
        if (so.MaxCost.HasValue)
            query = query.Where(x => x.Cost <= so.MaxCost.Value);

        return query;
    }
}
