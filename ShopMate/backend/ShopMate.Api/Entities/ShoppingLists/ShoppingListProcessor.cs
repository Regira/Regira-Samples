using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.ShoppingLists;

public class ShoppingListProcessor(ShopMateDbContext dbContext) : IEntityProcessor<ShoppingList, EntityIncludes>
{
    public async Task Process(IList<ShoppingList> items, EntityIncludes? includes, CancellationToken token = default)
    {
        var listIds = items.Select(x => x.Id).ToList();
        // IgnoreQueryFilters(): the archived-list filter on Article is also inlined here (it correlates
        // through ShoppingList.IsArchived), so a recompute for an archived list would otherwise see zero
        // articles even though restoring the list would immediately reveal them again - see
        // Regira.Entities entities.patterns -> Aggregates over a non-owned child collection.
        var counts = await dbContext.Articles
            .IgnoreQueryFilters()
            .Where(x => listIds.Contains(x.ShoppingListId))
            .GroupBy(x => x.ShoppingListId)
            .Select(g => new { ShoppingListId = g.Key, Total = g.Count(), Active = g.Count(a => a.IsActive) })
            .ToDictionaryAsync(x => x.ShoppingListId, token);

        foreach (var item in items)
        {
            if (counts.TryGetValue(item.Id, out var count))
            {
                item.ArticleCount = count.Total;
                item.ActiveArticleCount = count.Active;
            }
            else
            {
                item.ArticleCount = 0;
                item.ActiveArticleCount = 0;
            }
        }
    }
}
