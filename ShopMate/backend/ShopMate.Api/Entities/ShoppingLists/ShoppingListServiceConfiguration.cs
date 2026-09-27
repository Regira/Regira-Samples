using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.ShoppingLists;

public static class ShoppingListServiceConfiguration
{
    // simple registration with a search object (1 simple slot)
    public static EntityServiceCollection<ShopMateDbContext> AddShoppingLists(this IEntityServiceCollection<ShopMateDbContext> services)
        => services.For<ShoppingList, int, ShoppingListSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.ShopperId?.Any() == true)
                    query = query.Where(x => so.ShopperId.Contains(x.ShopperId));
                if (so?.IsPinned != null)
                    query = query.Where(x => x.IsPinned == so.IsPinned);
                return query;
            });
            e.SortBy(query => query.OrderByDescending(x => x.IsPinned).ThenBy(x => x.Title));
            // the owner is shown on every row -> unconditional to-one include
            e.Includes((query, _) => query.Include(x => x.Shopper));
            e.AddProcessor<ShoppingListProcessor>();
        });
}
