using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using ShoppingList.API.Data;

namespace ShoppingList.API.Entities.Shoppers;

public static class ShopperServiceConfiguration
{
    /// <summary>
    /// Registers the <see cref="Shopper"/> entity service. Uses the default search object,
    /// so the inherited <c>Q</c> parameter searches the normalized name/email via the global filter.
    /// </summary>
    public static IEntityServiceCollection<ShoppingDbContext> AddShoppers(this IEntityServiceCollection<ShoppingDbContext> services)
    {
        services.For<Shopper>(e => e.SortBy(query => query.OrderBy(x => x.Name)));
        return services;
    }
}
