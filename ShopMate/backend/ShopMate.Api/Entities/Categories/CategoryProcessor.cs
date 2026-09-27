using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.Categories;

/// <summary>Fills the number of articles directly tagged with each category.</summary>
public class CategoryProcessor(ShopMateDbContext dbContext) : IEntityProcessor<Category, EntityIncludes>
{
    public async Task Process(IList<Category> items, EntityIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.Id).ToList();
        var counts = await dbContext.ArticleCategories
            .Where(x => ids.Contains(x.CategoryId))
            .GroupBy(x => x.CategoryId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, token);
        foreach (var item in items)
            item.ArticleCount = counts.GetValueOrDefault(item.Id);
    }
}
