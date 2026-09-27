using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Categories;

/// <summary>Fills the number of active products per category (used by the storefront filter sidebar)</summary>
public class CategoryProcessor(WebshopDbContext dbContext) : IEntityProcessor<Category, EntityIncludes>
{
    public async Task Process(IList<Category> items, EntityIncludes? includes, CancellationToken token = default)
    {
        var ids = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Products
            .Where(p => p.IsActive && ids.Contains(p.CategoryId))
            .GroupBy(p => p.CategoryId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, token);
        foreach (var item in items)
            item.ProductCount = counts.GetValueOrDefault(item.Id);
    }
}
