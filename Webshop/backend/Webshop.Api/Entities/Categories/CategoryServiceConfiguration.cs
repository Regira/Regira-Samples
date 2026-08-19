using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Categories;

public static class CategoryServiceConfiguration
{
    // Budget: simple registration (1/5 simple)
    public static EntityServiceCollection<WebshopDbContext> AddCategories(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Category, int, CategorySearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.IsFeatured != null)
                    query = query.Where(x => x.IsFeatured == so.IsFeatured);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Title));
            e.AddProcessor<CategoryProcessor>();
        });
}
