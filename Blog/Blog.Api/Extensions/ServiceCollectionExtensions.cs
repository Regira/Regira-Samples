using Blog.Api.Data;
using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Posts;
using Blog.Api.Entities.Tags;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace Blog.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Budget tally (free tier = 5 simple + 2 complex):
    // | Entity       | Classification                          | Running tally  |
    // |--------------|-----------------------------------------|----------------|
    // | Category     | simple  (For<Category, int, SO>)        | 1/5 simple     |
    // | Tag          | simple  (For<Tag, int, SO>)             | 2/5 simple     |
    // | BlogPost     | complex (typed SortBy + Includes)       | 1/2 complex    |
    // | BlogPostTag  | owned m2m join via e.Related() - no slot | -             |
    // => 2 simple / 1 complex -> fits the free tier
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<BlogDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.DefaultPageSize = 24;
                options.MaxPageSize = 200;
            })
            .AddCategories()
            .AddTags()
            .AddBlogPosts();
}
