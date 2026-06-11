using Microsoft.EntityFrameworkCore;
using Regira.Entities.Processing.Abstractions;
using Webshop.API.Data;

namespace Webshop.API.Entities.Categories;

public class CategoryProcessor(WebshopDbContext dbContext) : IEntityProcessor<Category, CategoryIncludes>
{
    public async Task Process(IList<Category> items, CategoryIncludes? includes, CancellationToken token = default)
    {
        var itemIds = items.Select(x => x.Id).ToList();
        var counts = await dbContext.ProductCategories
            .Where(pc => itemIds.Contains(pc.CategoryId))
            .GroupBy(pc => pc.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, v => v.Count, token);
        foreach (var item in items)
            item.ProductCount = counts.TryGetValue(item.Id, out var count) ? count : 0;
    }
}
