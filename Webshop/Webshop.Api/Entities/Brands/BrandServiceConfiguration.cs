using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Brands;

public static class BrandServiceConfiguration
{
    // simple registration (1 simple slot)
    public static EntityServiceCollection<WebshopDbContext> AddBrands(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Brand>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
