using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Webshop.API.Data;

namespace Webshop.API.Entities.Categories;

public static class CategoryServiceConfiguration
{
    public static EntityServiceCollection<WebshopDbContext> AddCategories(
        this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Category, int, CategorySearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Q))
                    query = query.Where(x => x.NormalizedContent != null && x.NormalizedContent.Contains(so.Q));
                return query;
            });
        });
}
