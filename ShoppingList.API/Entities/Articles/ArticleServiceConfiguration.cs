using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using ShoppingList.API.Data;

namespace ShoppingList.API.Entities.Articles;

public static class ArticleServiceConfiguration
{
    /// <summary>
    /// Registers the <see cref="Article"/> entity service: text + category + brand filtering,
    /// sorting, category includes, and synchronization of the article-category links.
    /// </summary>
    public static IEntityServiceCollection<ShoppingDbContext> AddArticles(this IEntityServiceCollection<ShoppingDbContext> services)
    {
        services.For<Article, ArticleSearchObject, ArticleSortBy, EntityIncludes>(e =>
        {
            // Nested output projections + the InputDto->entity pair used by Related().
            e.AddMapping<ArticleDto, ArticleDto>();
            e.AddMapping<ArticleCategoryDto, ArticleCategoryDto>();
            e.AddMapping<ArticleCategoryInputDto, ArticleCategory>();

            e.AddFilter<ArticleQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                ArticleSortBy.Title => query.OrderBy(x => x.Title),
                ArticleSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
                ArticleSortBy.Newest => query.OrderByDescending(x => x.Created),
                ArticleSortBy.Oldest => query.OrderBy(x => x.Created),
                _ => query.OrderBy(x => x.Title)
            });
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(EntityIncludes.All) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(ac => ac.Category);
                return query;
            });
            e.Related(x => x.Categories);
        });
        return services;
    }
}
