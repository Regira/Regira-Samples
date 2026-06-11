using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using ShoppingList.API.Data;

namespace ShoppingList.API.Entities.Categories;

public static class CategoryServiceConfiguration
{
    /// <summary>
    /// Registers the <see cref="Category"/> entity service: hierarchy filtering, text search (Q),
    /// parent/child includes, an article-count processor, and synchronization of the self-referential links.
    /// </summary>
    public static IEntityServiceCollection<ShoppingDbContext> AddCategories(this IEntityServiceCollection<ShoppingDbContext> services)
    {
        services.For<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>(e =>
        {
            // Nested output projections + the InputDto->entity pair used by Related().
            e.AddMapping<CategoryCoreDto, CategoryCoreDto>();
            e.AddMapping<ParentCategoryDto, ParentCategoryDto>();
            e.AddMapping<ChildCategoryDto, ChildCategoryDto>();
            e.AddMapping<RelatedCategoryInputDto, RelatedCategory>();

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
            e.SortBy((query, _) => query.OrderBy(x => x.Title));
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(CategoryIncludes.Parents) == true)
                    query = query.Include(x => x.ParentEntities!).ThenInclude(x => x.Parent);
                if (includes?.HasFlag(CategoryIncludes.Children) == true)
                    query = query.Include(x => x.ChildEntities!).ThenInclude(x => x.Child);
                return query;
            });
            e.AddProcessor<CategoryProcessor>();
            e.Related(x => x.ParentEntities);
            e.Related(x => x.ChildEntities);
        });
        return services;
    }
}
