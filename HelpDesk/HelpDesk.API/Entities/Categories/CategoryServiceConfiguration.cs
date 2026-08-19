using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using HelpDesk.API.Data;

namespace HelpDesk.API.Entities.Categories;

public static class CategoryServiceConfiguration
{
    // 2/5 simple
    public static EntityServiceCollection<AppDbContext> AddCategories(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Category>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
