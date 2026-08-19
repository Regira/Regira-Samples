using Blog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace Blog.Api.Entities.BlogPosts;

public static class BlogPostServiceConfiguration
{
    // complex registration - 1 complex slot (typed sorting + includes for filtering by category/tag/published)
    public static EntityServiceCollection<BlogDbContext> AddBlogPosts(this IEntityServiceCollection<BlogDbContext> services)
        => services.For<BlogPost, BlogPostSearchObject, BlogPostSortBy, BlogPostIncludes>(e =>
        {
            e.AddFilter<BlogPostQueryBuilder>();

            e.SortBy((query, sortBy) => sortBy switch
            {
                BlogPostSortBy.Title => query.OrderOrThenBy(x => x.Title),
                BlogPostSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                BlogPostSortBy.PublishedAt => query.OrderOrThenBy(x => x.PublishedAt),
                BlogPostSortBy.PublishedAtDesc => query.OrderOrThenByDescending(x => x.PublishedAt),
                _ => query.OrderOrThenByDescending(x => x.PublishedAt)
            });

            // Category is a to-one shown on every row -> eager-load unconditionally.
            // Tags is a collection -> flag-gated behind BlogPostIncludes.Tags (always loaded on Details).
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Category);
                if (includes?.HasFlag(BlogPostIncludes.Tags) == true)
                {
                    query = query.Include(x => x.Tags!).ThenInclude(t => t.Tag);
                }
                return query;
            });

            e.Related(x => x.Tags);
        });
}
