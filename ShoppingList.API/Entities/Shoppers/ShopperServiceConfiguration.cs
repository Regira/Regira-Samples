using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using ShoppingListApi.Data;

namespace ShoppingListApi.Entities.Shoppers;

public static class ShopperServiceConfiguration
{
    public static EntityServiceCollection<ShoppingListDbContext> AddShoppers(
        this IEntityServiceCollection<ShoppingListDbContext> services)
        => services.For<Shopper, int, ShopperSearchObject>(e =>
        {
            // Simple builder → single-arg SortBy lambda.
            e.SortBy(query => query.OrderBy(x => x.Name));

            e.Filter((query, so) => string.IsNullOrWhiteSpace(so?.Name)
                ? query
                : query.Where(x => x.Name.Contains(so.Name)));
        });
}
