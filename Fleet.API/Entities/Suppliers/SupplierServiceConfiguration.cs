using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.API.Entities.Suppliers;

public static class SupplierServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddSuppliers(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Supplier, int, SupplierSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (!string.IsNullOrWhiteSpace(so.Title))
                    query = query.Where(x => x.Title.Contains(so.Title));
                if (!string.IsNullOrWhiteSpace(so.City))
                    query = query.Where(x => x.City != null && x.City.Contains(so.City));
                if (so.InterventionTypeId?.Any() == true)
                    query = query.Where(x => x.Capabilities!.Any(c => so.InterventionTypeId.Contains(c.InterventionTypeId)));
                return query;
            });
            // Always eager-load the supplier's capabilities.
            e.Includes((query, _) => query
                .Include(x => x.Capabilities!)
                .ThenInclude(c => c.InterventionType));

            e.AddMapping<SupplierInterventionTypeDto, SupplierInterventionTypeDto>();
            e.AddMapping<SupplierInterventionTypeInputDto, SupplierInterventionType>();
            e.Related<SupplierInterventionType>(x => x.Capabilities);
        });
}
