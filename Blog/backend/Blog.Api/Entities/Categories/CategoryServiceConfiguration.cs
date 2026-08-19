using Blog.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Blog.Api.Entities.Categories;

public static class CategoryServiceConfiguration
{
    // simple registration - 1 simple slot
    public static EntityServiceCollection<BlogDbContext> AddCategories(this IEntityServiceCollection<BlogDbContext> services)
        => services.For<Category>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.AddProcessor<CategoryProcessor>();
        });
}
