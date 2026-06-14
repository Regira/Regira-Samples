using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using ShoppingListApi.Data;

namespace ShoppingListApi.Entities.Articles;

public static class ArticleServiceConfiguration
{
    public static EntityServiceCollection<ShoppingListDbContext> AddArticles(
        this IEntityServiceCollection<ShoppingListDbContext> services)
        => services.For<Article, ArticleSearchObject, ArticleSortBy, EntityIncludes>(e =>
        {
            e.UseMapping<ArticleDto, ArticleInputDto>();
            e.AddMapping<ArticleCategoryDto, ArticleCategoryDto>();     // nested output collection
            e.AddMapping<ArticleCategoryInputDto, ArticleCategory>();   // input items synced via Related()

            e.AddFilter<ArticleQueryBuilder>();

            e.SortBy((query, sortBy) => sortBy switch
            {
                ArticleSortBy.Title => query.OrderBy(x => x.Title),
                ArticleSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
                ArticleSortBy.Brand => query.OrderBy(x => x.Brand),
                ArticleSortBy.BrandDesc => query.OrderByDescending(x => x.Brand),
                ArticleSortBy.Newest => query.OrderByDescending(x => x.Created),
                _ => query.OrderBy(x => x.Title)
            });

            // Owned join collection — synchronized through this service.
            e.Related(x => x.Categories);

            // Eager-load categories on demand (?includes=All).
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(EntityIncludes.All) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(ac => ac.Category);
                return query;
            });
        });
}
