using Fleet.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace Fleet.Api.Entities.Suppliers;

public static class SupplierServiceConfiguration
{
    // Budget: simple (2/5 simple)
    public static EntityServiceCollection<FleetDbContext> AddSuppliers(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Supplier, int, SupplierSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Title)) query = query.Where(x => x.Title!.Contains(so.Title));
                if (so?.IsActive != null) query = query.Where(x => x.IsActive == so.IsActive);
                if (so?.InterventionTypeId?.Any() == true)
                    query = query.Where(x => x.SupportedInterventionTypes!.Any(sit => so.InterventionTypeId.Contains(sit.InterventionTypeId)));
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.Includes((query, includes) => includes?.HasFlag(EntityIncludes.All) == true
                ? query.Include(x => x.SupportedInterventionTypes!).ThenInclude(x => x.InterventionType)
                : query);
            e.Related(x => x.SupportedInterventionTypes);
        });
}
