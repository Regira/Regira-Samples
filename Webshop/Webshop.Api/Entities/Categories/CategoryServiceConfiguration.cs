using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Categories;

public static class CategoryServiceConfiguration
{
    // simple registration (1 simple slot): one fixed sort, default SearchObject (id/ids/q/...)
    public static EntityServiceCollection<WebshopDbContext> AddCategories(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Category>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
            e.AddProcessor<CategoryProcessor>();
        });
}
