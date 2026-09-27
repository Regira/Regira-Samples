using Blog.Api.Data;
using Blog.Api.Services;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Blog.Api.Entities.Tags;

public static class TagServiceConfiguration
{
    // simple registration (1 simple slot)
    public static EntityServiceCollection<BlogDbContext> AddTags(this IEntityServiceCollection<BlogDbContext> services)
        => services.For<Tag, int, TagSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Slug))
                    query = query.Where(x => x.Slug == so.Slug);
                if (so?.HasPublishedPosts == true)
                {
                    var now = DateTime.UtcNow;
                    query = query.Where(x => x.PostTags!.Any(pt => pt.Post!.IsPublished && pt.Post.PublishedAt != null && pt.Post.PublishedAt <= now));
                }
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.AddPrepper<SlugPrepper<Tag>>();
            e.AddProcessor<TagProcessor>();
        });
}
