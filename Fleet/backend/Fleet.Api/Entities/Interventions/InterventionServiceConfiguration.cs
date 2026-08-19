using Fleet.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace Fleet.Api.Entities.Interventions;

public static class InterventionServiceConfiguration
{
    // Budget: complex (1/2 complex)
    public static EntityServiceCollection<FleetDbContext> AddInterventions(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.VehicleId?.Any() == true) query = query.Where(x => so.VehicleId.Contains(x.VehicleId));
                if (so?.SupplierId?.Any() == true) query = query.Where(x => so.SupplierId.Contains(x.SupplierId));
                if (so?.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (so?.InvoiceId?.Any() == true) query = query.Where(x => x.InvoiceId != null && so.InvoiceId.Contains(x.InvoiceId.Value));
                if (so?.HasInvoice != null) query = so.HasInvoice.Value ? query.Where(x => x.InvoiceId != null) : query.Where(x => x.InvoiceId == null);
                if (so?.InterventionTypeId?.Any() == true)
                    query = query.Where(x => x.InterventionTypes!.Any(it => so.InterventionTypeId.Contains(it.InterventionTypeId)));
                if (so?.MinScheduledDate != null) query = query.Where(x => x.ScheduledDate >= so.MinScheduledDate.Value);
                if (so?.MaxScheduledDate != null) query = query.Where(x => x.ScheduledDate <= so.MaxScheduledDate.Value);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                InterventionSortBy.ScheduledDate => query.OrderOrThenBy(x => x.ScheduledDate),
                InterventionSortBy.ScheduledDateDesc => query.OrderOrThenByDescending(x => x.ScheduledDate),
                InterventionSortBy.Status => query.OrderOrThenBy(x => x.Status),
                InterventionSortBy.Cost => query.OrderOrThenBy(x => x.Cost),
                InterventionSortBy.CostDesc => query.OrderOrThenByDescending(x => x.Cost),
                _ => query.OrderOrThenByDescending(x => x.ScheduledDate)
            });
            // Vehicle + Supplier: cheap to-one references shown on every row -> unconditional.
            // InterventionTypes: a collection -> flag-gated.
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Vehicle!).Include(x => x.Supplier!).Include(x => x.Invoice!);
                if (includes?.HasFlag(InterventionIncludes.InterventionTypes) == true)
                    query = query.Include(x => x.InterventionTypes!).ThenInclude(x => x.InterventionType);
                return query;
            });
            e.Related(x => x.InterventionTypes);
            e.AddPrepper<InterventionInvoiceTotalPrepper>();
        });
}
