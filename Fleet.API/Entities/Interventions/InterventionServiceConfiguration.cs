using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.API.Entities.Interventions;

public static class InterventionServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddInterventions(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes>(e =>
        {
            e.Filter((query, so) =>
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
                if (so.HasInvoice.HasValue)
                    query = so.HasInvoice.Value
                        ? query.Where(x => x.InvoiceId != null)
                        : query.Where(x => x.InvoiceId == null);
                if (so.Status?.Any() == true)
                    query = query.Where(x => so.Status.Contains(x.Status));
                if (so.MinScheduledDate.HasValue)
                    query = query.Where(x => x.ScheduledDate >= so.MinScheduledDate.Value);
                if (so.MaxScheduledDate.HasValue)
                    query = query.Where(x => x.ScheduledDate <= so.MaxScheduledDate.Value);
                if (so.MinCost.HasValue)
                    query = query.Where(x => x.Cost >= so.MinCost.Value);
                if (so.MaxCost.HasValue)
                    query = query.Where(x => x.Cost <= so.MaxCost.Value);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                InterventionSortBy.ScheduledDate => query.OrderBy(x => x.ScheduledDate),
                InterventionSortBy.ScheduledDateDesc => query.OrderByDescending(x => x.ScheduledDate),
                InterventionSortBy.CompletedDate => query.OrderBy(x => x.CompletedDate),
                InterventionSortBy.CompletedDateDesc => query.OrderByDescending(x => x.CompletedDate),
                InterventionSortBy.Cost => query.OrderBy(x => x.Cost),
                InterventionSortBy.CostDesc => query.OrderByDescending(x => x.Cost),
                InterventionSortBy.Status => query.OrderBy(x => x.Status),
                _ => query.OrderByDescending(x => x.ScheduledDate)
            });
            e.Includes((query, includes) =>
            {
                // Vehicle, supplier and type are core context — always loaded.
                query = query
                    .Include(x => x.Vehicle)
                    .Include(x => x.Supplier)
                    .Include(x => x.InterventionType);
                if (includes?.HasFlag(InterventionIncludes.Invoice) == true)
                    query = query.Include(x => x.Invoice);
                return query;
            });
        });
}
