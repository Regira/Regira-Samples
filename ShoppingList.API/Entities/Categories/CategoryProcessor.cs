using Microsoft.EntityFrameworkCore;
using Regira.Entities.Processing.Abstractions;
using ShoppingList.API.Data;

namespace ShoppingList.API.Entities.Categories;

/// <summary>
/// Fills the <see cref="Category.ArticleCount"/> (a <c>[NotMapped]</c> property) after categories
/// are fetched from the database, by counting the related article links.
/// </summary>
public class CategoryProcessor(ShoppingDbContext dbContext) : IEntityProcessor<Category, CategoryIncludes>
{
    public async Task Process(IList<Category> items, CategoryIncludes? includes, CancellationToken token = default)
    {
        var itemIds = items.Select(x => x.Id).ToList();
        var counts = await dbContext.ArticleCategories
            .Where(ac => itemIds.Contains(ac.CategoryId))
            .GroupBy(ac => ac.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, token);

        foreach (var item in items)
            item.ArticleCount = counts.TryGetValue(item.Id, out var count) ? count : 0;
    }
}
