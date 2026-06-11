using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

namespace Fleet.API.Entities.Suppliers;

public static class SupplierConfiguration
{
    public static IEntityServiceCollection<FleetDbContext> AddSuppliers(this IEntityServiceCollection<FleetDbContext> services)
    {
        // Simple registration: default SearchObject (Q full-text search on company name / contact / email).
        services.For<Supplier>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.Includes((query, _) => query
                .Include(x => x.Capabilities!)
                .ThenInclude(c => c.InterventionType));
            // The capability join collection is owned by the supplier and synced via Related().
            e.Related<SupplierInterventionType, int>(x => x.Capabilities);
        });
        return services;
    }
}
