using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.ShoppingLists;

/// <summary>Fills the article counters shown on every list card.</summary>
public class ShoppingListProcessor(ShopMateDbContext dbContext) : IEntityProcessor<ShoppingList, EntityIncludes>
{
    public async Task Process(IList<ShoppingList> items, EntityIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Articles
            .Where(a => ids.Contains(a.ShoppingListId))
            .GroupBy(a => a.ShoppingListId)
            .Select(g => new { Id = g.Key, Total = g.Count(), Active = g.Count(a => a.IsActive) })
            .ToDictionaryAsync(x => x.Id, token);
        foreach (var item in items)
        {
            var c = counts.GetValueOrDefault(item.Id);
            item.ArticleCount = c?.Total ?? 0;
            item.ActiveCount = c?.Active ?? 0;
        }
    }
}
