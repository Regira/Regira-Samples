using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using ShoppingListApi.Data;

namespace ShoppingListApi.Entities.Categories;

public static class CategoryServiceConfiguration
{
    public static EntityServiceCollection<ShoppingListDbContext> AddCategories(
        this IEntityServiceCollection<ShoppingListDbContext> services)
        => services.For<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>(e =>
        {
            // DTO mapping — top-level pair + the nested related collections (read & write).
            e.UseMapping<CategoryDto, CategoryInputDto>();
            e.AddMapping<ParentCategoryDto, ParentCategoryDto>();   // nested output collection
            e.AddMapping<ChildCategoryDto, ChildCategoryDto>();     // nested output collection
            e.AddMapping<RelatedCategoryInputDto, RelatedCategory>(); // input items synced via Related()

            // Filtering (Q full-text search is provided globally for IHasNormalizedContent).
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

            e.SortBy((query, sortBy) => query.OrderBy(x => x.Title));

            // Eager-load the hierarchy on demand (?includes=Parents / Children / All).
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(CategoryIncludes.Parents) == true)
                    query = query.Include(x => x.ParentEntities!).ThenInclude(rc => rc.Parent);
                if (includes?.HasFlag(CategoryIncludes.Children) == true)
                    query = query.Include(x => x.ChildEntities!).ThenInclude(rc => rc.Child);
                return query;
            });

            e.AddProcessor<CategoryProcessor>();

            // Owned join collections — synchronized through this service.
            e.Related(x => x.ParentEntities);
            e.Related(x => x.ChildEntities);
        });
}
