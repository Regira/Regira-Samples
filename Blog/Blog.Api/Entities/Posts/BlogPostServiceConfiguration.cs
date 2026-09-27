using Blog.Api.Data;
using Blog.Api.Services;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace Blog.Api.Entities.Posts;

public static class BlogPostServiceConfiguration
{
    // complex registration (1 complex slot): typed sorting + includes, batch POST /list and /search
    public static EntityServiceCollection<BlogDbContext> AddBlogPosts(this IEntityServiceCollection<BlogDbContext> services)
        => services.For<BlogPost, int, BlogPostSearchObject, BlogPostSortBy, BlogPostIncludes>(e =>
        {
            e.AddFilter<BlogPostQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                BlogPostSortBy.Oldest => query.OrderOrThenBy(x => x.PublishedAt ?? x.Created),
                BlogPostSortBy.Title => query.OrderOrThenBy(x => x.Title),
                BlogPostSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                BlogPostSortBy.ReadingTime => query.OrderOrThenBy(x => x.ReadingTimeMinutes),
                BlogPostSortBy.ReadingTimeDesc => query.OrderOrThenByDescending(x => x.ReadingTimeMinutes),
                BlogPostSortBy.LastModified => query.OrderOrThenByDescending(x => x.LastModified ?? x.Created),
                BlogPostSortBy.Created => query.OrderOrThenByDescending(x => x.Created),
                // Default + Newest: latest publication first, drafts (no date) by creation
                _ => query.OrderOrThenByDescending(x => x.PublishedAt ?? x.Created)
            });
            // ONE Includes registration: the category (to-one, shown on every card) always,
            // the tag collection only when the client opts in (?includes=Tags) - Details loads everything
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Category);
                if (includes?.HasFlag(BlogPostIncludes.Tags) == true)
                    query = query.Include(x => x.Tags!).ThenInclude(t => t.Tag);
                return query;
            });
            e.AddPrepper<SlugPrepper<BlogPost>>();
            e.AddPrepper<BlogPostPrepper>();
            e.Related(x => x.Tags);
        });
}
