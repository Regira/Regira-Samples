using Blog.Api.Data;
using Blog.Api.Services;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Blog.Api.Entities.Categories;

public static class CategoryServiceConfiguration
{
    // simple registration (1 simple slot)
    public static EntityServiceCollection<BlogDbContext> AddCategories(this IEntityServiceCollection<BlogDbContext> services)
        => services.For<Category, int, CategorySearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Slug))
                    query = query.Where(x => x.Slug == so.Slug);
                if (so?.HasPublishedPosts == true)
                {
                    var now = DateTime.UtcNow;
                    query = query.Where(x => x.Posts!.Any(p => p.IsPublished && p.PublishedAt != null && p.PublishedAt <= now));
                }
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
            e.AddPrepper<SlugPrepper<Category>>();
            e.AddProcessor<CategoryProcessor>();
        });
}
