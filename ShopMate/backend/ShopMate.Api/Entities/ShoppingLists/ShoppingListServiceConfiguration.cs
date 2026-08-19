using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.ShoppingLists;

public static class ShoppingListServiceConfiguration
{
    public static EntityServiceCollection<ShopMateDbContext> AddShoppingLists(this IEntityServiceCollection<ShopMateDbContext> services)
        => services.For<ShoppingList, int, ShoppingListSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.OwnerName))
                    query = query.Where(x => x.OwnerName != null && x.OwnerName.Contains(so.OwnerName));
                return query;
            });
            e.SortBy(query => query.OrderByDescending(x => x.LastModified ?? x.Created));
            e.AddProcessor<ShoppingListProcessor>();
        });
}
