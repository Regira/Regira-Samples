using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using ShoppingList.API.Data;
using ShoppingList.API.Entities.ShoppingListItems;

namespace ShoppingList.API.Entities.ShoppingLists;

public static class ShoppingListServiceConfiguration
{
    /// <summary>
    /// Registers the <see cref="ShoppingList"/> entity service: filtering by shopper and always
    /// eager-loading the list items together with their articles.
    /// </summary>
    public static IEntityServiceCollection<ShoppingDbContext> AddShoppingLists(this IEntityServiceCollection<ShoppingDbContext> services)
    {
        services.For<ShoppingList, int, ShoppingListSearchObject>(e =>
        {
            // Nested output projection of the list items.
            e.AddMapping<ShoppingListItemDto, ShoppingListItemDto>();

            e.Filter((query, so) =>
            {
                if (so?.ShopperId?.Any() == true)
                    query = query.Where(x => so.ShopperId.Contains(x.ShopperId));

                // A list is only meaningful together with its items, so eager-load them.
                return query
                    .Include(x => x.Items!)
                    .ThenInclude(i => i.Article);
            });
            e.SortBy(query => query.OrderBy(x => x.Name));
        });
        return services;
    }
}
