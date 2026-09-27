using Blog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;

namespace Blog.Api.Entities.Categories;

public class CategoryProcessor(BlogDbContext dbContext) : IEntityProcessor<Category, EntityIncludes>
{
    public async Task Process(IList<Category> items, EntityIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.Id).ToList();
        var now = DateTime.UtcNow;
        var counts = await dbContext.BlogPosts
            .Where(p => p.CategoryId != null && ids.Contains(p.CategoryId.Value))
            .GroupBy(p => p.CategoryId!.Value)
            .Select(g => new
            {
                CategoryId = g.Key,
                Total = g.Count(),
                Published = g.Count(p => p.IsPublished && p.PublishedAt != null && p.PublishedAt <= now)
            })
            .ToDictionaryAsync(x => x.CategoryId, token);
        foreach (var item in items)
        {
            counts.TryGetValue(item.Id, out var c);
            item.PostCount = c?.Total ?? 0;
            item.PublishedPostCount = c?.Published ?? 0;
        }
    }
}
