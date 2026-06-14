using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Extensions;
using ShoppingListApi.Data;

namespace ShoppingListApi.Entities.Lists;

public static class ShoppingListServiceConfiguration
{
    public static EntityServiceCollection<ShoppingListDbContext> AddShoppingLists(
        this IEntityServiceCollection<ShoppingListDbContext> services)
        => services.For<ShoppingList, int, ShoppingListSearchObject>(e =>
        {
            e.UseMapping<ShoppingListDto, ShoppingListInputDto>()
                .After((entity, dto) =>
                {
                    dto.ItemCount = entity.Items?.Count ?? 0;
                    dto.ActiveItemCount = entity.Items?.Count(i => i.IsActive) ?? 0;
                });
            e.AddMapping<ShoppingListItemDto, ShoppingListItemDto>();     // nested output collection
            e.AddMapping<ShoppingListItemInputDto, ShoppingListItem>();   // input items synced via Related()

            e.Filter((query, so) => so?.ShopperId?.Any() == true
                ? query.Where(x => so.ShopperId.Contains(x.ShopperId))
                : query);

            // Simple builder → single-arg SortBy lambda.
            e.SortBy(query => query.OrderByDescending(x => x.Created));

            // Always eager-load the owning shopper and the items (+ their article) for a usable view.
            e.Includes((query, _) => query
                .Include(x => x.Shopper)
                .Include(x => x.Items!.OrderBy(i => i.SortOrder))
                    .ThenInclude(i => i.Article));

            // Owned item collection — synchronized through this service; keeps SortOrder contiguous.
            e.Related(x => x.Items, item => item.Items?.SetSortOrder());
        });
}
