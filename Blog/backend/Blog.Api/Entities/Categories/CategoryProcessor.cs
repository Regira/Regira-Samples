using Blog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;

namespace Blog.Api.Entities.Categories;

public class CategoryProcessor(BlogDbContext dbContext) : IEntityProcessor<Category, EntityIncludes>
{
    public async Task Process(IList<Category> items, EntityIncludes? includes, CancellationToken token = default)
    {
        var ids = items.Select(x => x.Id).ToList();
        var counts = await dbContext.Categories
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, Count = dbContext.BlogPosts.Count(p => p.CategoryId == x.Id) })
            .ToDictionaryAsync(x => x.Id, v => v.Count, token);

        foreach (var item in items)
        {
            item.PostCount = counts.TryGetValue(item.Id, out var count) ? count : 0;
        }
    }
}
