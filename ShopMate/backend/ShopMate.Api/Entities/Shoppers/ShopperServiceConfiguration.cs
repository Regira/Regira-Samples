using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.Shoppers;

public static class ShopperServiceConfiguration
{
    // simple registration (1 simple slot)
    public static EntityServiceCollection<ShopMateDbContext> AddShoppers(this IEntityServiceCollection<ShopMateDbContext> services)
        => services.For<Shopper>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Name));
        });
}
