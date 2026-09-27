using Fleet.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace Fleet.Api.Entities.Interventions;

public static class InterventionServiceConfiguration
{
    // complex: typed sorting + opt-in Lines include for the work-order data table
    public static EntityServiceCollection<FleetDbContext> AddInterventions(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.VehicleId?.Any() == true) query = query.Where(x => so.VehicleId.Contains(x.VehicleId));
                if (so.SupplierId?.Any() == true) query = query.Where(x => so.SupplierId.Contains(x.SupplierId));
                if (so.InvoiceId?.Any() == true) query = query.Where(x => x.InvoiceId != null && so.InvoiceId.Contains(x.InvoiceId.Value));
                if (so.InterventionTypeId?.Any() == true)
                    query = query.Where(x => x.Lines!.Any(l => so.InterventionTypeId.Contains(l.InterventionTypeId)));
                if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (so.Priority?.Any() == true) query = query.Where(x => so.Priority.Contains(x.Priority));
                if (so.IsInvoiced.HasValue) query = query.Where(x => (x.InvoiceId != null) == so.IsInvoiced);
                if (so.MinDate.HasValue) query = query.Where(x => x.ScheduledDate >= so.MinDate);
                if (so.MaxDate.HasValue) query = query.Where(x => x.ScheduledDate <= so.MaxDate);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                InterventionSortBy.ScheduledDate => query.OrderOrThenBy(x => x.ScheduledDate),
                InterventionSortBy.ScheduledDateDesc => query.OrderOrThenByDescending(x => x.ScheduledDate),
                // SQLite cannot ORDER BY decimal -> sort on a REAL projection
                InterventionSortBy.TotalCost => query.OrderOrThenBy(x => (double)x.TotalCost),
                InterventionSortBy.TotalCostDesc => query.OrderOrThenByDescending(x => (double)x.TotalCost),
                InterventionSortBy.Status => query.OrderOrThenBy(x => x.Status),
                InterventionSortBy.Priority => query.OrderOrThenBy(x => x.Priority),
                InterventionSortBy.PriorityDesc => query.OrderOrThenByDescending(x => x.Priority),
                InterventionSortBy.Vehicle => query.OrderOrThenBy(x => x.Vehicle!.LicensePlate),
                InterventionSortBy.Supplier => query.OrderOrThenBy(x => x.Supplier!.Title),
                _ => query.OrderOrThenByDescending(x => x.ScheduledDate).ThenByDescending(x => x.Id)
            });
            e.Includes((query, includes) =>
            {
                // to-one references shown on every row: unconditional
                query = query
                    .Include(x => x.Vehicle)
                    .Include(x => x.Supplier)
                    .Include(x => x.Invoice);
                // the owned collection: opt-in (?includes=Lines), always on Details
                if (includes?.HasFlag(InterventionIncludes.Lines) == true)
                    query = query.Include(x => x.Lines!).ThenInclude(l => l.InterventionType);
                return query;
            });
            e.Related<InterventionLine>(x => x.Lines);
            e.AddPrepper<InterventionPrepper>();
            e.AddNormalizer<InterventionNormalizer>();
            e.AddReactor<InterventionInvoiceReactor>();
        });
}
