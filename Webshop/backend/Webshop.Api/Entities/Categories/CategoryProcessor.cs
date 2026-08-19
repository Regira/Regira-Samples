using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Categories;

public class CategoryProcessor(WebshopDbContext dbContext) : IEntityProcessor<Category, EntityIncludes>
{
    public async Task Process(IList<Category> items, EntityIncludes? includes, CancellationToken token = default)
    {
        var itemIds = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Products
            .Where(p => itemIds.Contains(p.CategoryId))
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, token);

        foreach (var item in items)
            item.ProductCount = counts.GetValueOrDefault(item.Id, 0);
    }
}
