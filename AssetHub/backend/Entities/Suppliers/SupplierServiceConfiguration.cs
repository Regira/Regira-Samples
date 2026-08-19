using AssetHub.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace AssetHub.Api.Entities.Suppliers;

// Budget: simple 4/5
public static class SupplierServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddSuppliers(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Supplier, int, SupplierSearchObject>(e =>
        {
            e.Filter((query, so) => string.IsNullOrWhiteSpace(so?.Title)
                ? query
                : query.Where(x => x.Title.Contains(so.Title)));
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
