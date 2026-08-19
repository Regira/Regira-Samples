using AssetHub.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace AssetHub.Api.Entities.Categories;

// Budget: simple 1/5
public static class CategoryServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddCategories(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Category, int, CategorySearchObject>(e =>
        {
            e.Filter((query, so) => string.IsNullOrWhiteSpace(so?.Title)
                ? query
                : query.Where(x => x.Title.Contains(so.Title)));
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
