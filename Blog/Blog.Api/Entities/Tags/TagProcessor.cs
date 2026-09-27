using Blog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;

namespace Blog.Api.Entities.Tags;

public class TagProcessor(BlogDbContext dbContext) : IEntityProcessor<Tag, EntityIncludes>
{
    public async Task Process(IList<Tag> items, EntityIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.Id).ToList();
        var now = DateTime.UtcNow;
        var counts = await dbContext.BlogPostTags
            .Where(pt => ids.Contains(pt.TagId))
            .GroupBy(pt => pt.TagId)
            .Select(g => new
            {
                TagId = g.Key,
                Total = g.Count(),
                Published = g.Count(pt => pt.Post!.IsPublished && pt.Post.PublishedAt != null && pt.Post.PublishedAt <= now)
            })
            .ToDictionaryAsync(x => x.TagId, token);
        foreach (var item in items)
        {
            counts.TryGetValue(item.Id, out var c);
            item.PostCount = c?.Total ?? 0;
            item.PublishedPostCount = c?.Published ?? 0;
        }
    }
}
