using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using ShoppingList.API.Data;

namespace ShoppingList.API.Entities.ShoppingListItems;

public static class ShoppingListItemServiceConfiguration
{
    /// <summary>
    /// Registers the <see cref="ShoppingListItem"/> entity service with its dedicated query builder
    /// (list/article/active/category/text filtering + article eager-loading).
    /// </summary>
    public static IEntityServiceCollection<ShoppingDbContext> AddShoppingListItems(this IEntityServiceCollection<ShoppingDbContext> services)
    {
        services.For<ShoppingListItem, int, ShoppingListItemSearchObject>(e =>
        {
            e.AddFilter<ShoppingListItemQueryBuilder>();
            e.SortBy(query => query.OrderBy(x => x.Id));
        });
        return services;
    }
}
