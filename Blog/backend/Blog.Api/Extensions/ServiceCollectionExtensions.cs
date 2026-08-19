using Blog.Api.Data;
using Blog.Api.Entities.BlogPosts;
using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Tags;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace Blog.Api.Extensions;

// Entity budget tally (free tier = 5 simple + 2 complex):
// - Category    -> simple  (1/5 simple)
// - Tag         -> simple  (2/5 simple)
// - BlogPost    -> complex, typed SortBy + Includes (1/2 complex)
// - BlogPostTag -> owned child via e.Related() on BlogPost - no slot
// => 2 simple / 1 complex registered -> fits free tier comfortably
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<BlogDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddCategories()
            .AddTags()
            .AddBlogPosts();
}
