using Fleet.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.Api.Entities.Suppliers;

public static class SupplierServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddSuppliers(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Supplier, int, SupplierSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.InterventionTypeId?.Any() == true)
                    query = query.Where(x => x.InterventionTypes!.Any(t => so.InterventionTypeId.Contains(t.InterventionTypeId)));
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive);
                if (!string.IsNullOrWhiteSpace(so.City)) query = query.Where(x => x.City == so.City);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
            // Capabilities are shown on every supplier row (simple entity -> no ?includes=), so the one
            // small collection is loaded unconditionally.
            e.Includes((query, _) => query.Include(x => x.InterventionTypes!).ThenInclude(x => x.InterventionType));
            e.Related<SupplierInterventionType>(x => x.InterventionTypes);
        });
}
