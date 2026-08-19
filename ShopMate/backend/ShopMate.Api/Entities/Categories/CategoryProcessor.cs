using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.Categories;

public class CategoryProcessor(ShopMateDbContext dbContext) : IEntityProcessor<Category, EntityIncludes>
{
    public async Task Process(IList<Category> items, EntityIncludes? includes, CancellationToken token = default)
    {
        var categoryIds = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Categories
            .Where(x => categoryIds.Contains(x.Id))
            .Select(x => new { x.Id, Count = dbContext.ArticleCategories.Count(ac => ac.CategoryId == x.Id) })
            .ToDictionaryAsync(x => x.Id, v => v.Count, token);

        foreach (var item in items)
            item.ArticleCount = counts.TryGetValue(item.Id, out var count) ? count : 0;
    }
}
