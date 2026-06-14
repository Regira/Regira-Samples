using Microsoft.EntityFrameworkCore;
using Regira.Entities.Processing.Abstractions;
using ShoppingListApi.Data;

namespace ShoppingListApi.Entities.Categories;

/// <summary>
/// Fills the <see cref="Category.ArticleCount"/> derived (<c>[NotMapped]</c>) property after a
/// category is fetched, by counting the article-category join rows per category.
/// </summary>
public class CategoryProcessor(ShoppingListDbContext dbContext) : IEntityProcessor<Category, CategoryIncludes>
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
