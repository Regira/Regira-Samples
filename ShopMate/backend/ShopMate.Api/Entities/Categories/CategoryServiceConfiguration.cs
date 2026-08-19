using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.Categories;

public static class CategoryServiceConfiguration
{
    public static EntityServiceCollection<ShopMateDbContext> AddCategories(this IEntityServiceCollection<ShopMateDbContext> services)
        => services.For<Category, int, CategorySearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.ParentId?.Any() == true)
                    query = query.Where(x => x.ParentEntities!.Any(pe => so.ParentId.Contains(pe.ParentId)));
                if (so?.ChildId?.Any() == true)
                    query = query.Where(x => x.ChildEntities!.Any(ce => so.ChildId.Contains(ce.ChildId)));
                if (so?.IsRoot != null)
                    query = so.IsRoot.Value
                        ? query.Where(x => !x.ParentEntities!.Any())
                        : query.Where(x => x.ParentEntities!.Any());
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.Includes((query, _) => query
                .Include(x => x.ParentEntities!).ThenInclude(x => x.Parent)
                .Include(x => x.ChildEntities!).ThenInclude(x => x.Child)
                .AsSplitQuery());
            e.AddProcessor<CategoryProcessor>();
            e.Related(x => x.ParentEntities);
            e.Related(x => x.ChildEntities);
        });
}
