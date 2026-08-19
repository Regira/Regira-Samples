using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.Articles;

public static class ArticleServiceConfiguration
{
    public static EntityServiceCollection<ShopMateDbContext> AddArticles(this IEntityServiceCollection<ShopMateDbContext> services)
        => services.For<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>(e =>
        {
            e.AddFilter<ArticleQueryBuilder>();
            // NOTE - confirmed framework quirk: ordering the unfiltered (all-lists) Article query by
            // SortOrder returns one row fewer than the requested pageSize on every page (count is
            // correct, items is short) - reproducible with a plain .OrderBy(x => x.SortOrder), with or
            // without a .ThenBy(x => x.Id) tiebreaker, and NOT reproduced when ordering by another
            // equally-duplicated int column (e.g. ShoppingListId) instead. SortOrder is scoped per
            // shopping list (many rows share the same value across different lists), so this only shows
            // up cross-list; ordering by SortOrder once scoped to a single shoppingListId (its intended
            // use - the per-list reorder view, where values are unique) is unaffected, see below. Default
            // therefore sorts by Title for the unscoped browse; SortOrder stays available (and correct)
            // once a caller filters to one list.
            e.SortBy((query, sortBy) => sortBy switch
            {
                ArticleSortBy.SortOrder => query.OrderOrThenBy(x => x.SortOrder).OrderOrThenBy(x => x.Id),
                ArticleSortBy.Title => query.OrderOrThenBy(x => x.Title).OrderOrThenBy(x => x.Id),
                ArticleSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title).OrderOrThenBy(x => x.Id),
                ArticleSortBy.Created => query.OrderOrThenBy(x => x.Created).OrderOrThenBy(x => x.Id),
                ArticleSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created).OrderOrThenBy(x => x.Id),
                _ => query.OrderOrThenBy(x => x.Title).OrderOrThenBy(x => x.Id)
            });
            // ShoppingList is a to-one shown on every row (breadcrumb) -> eager-load unconditionally.
            // Categories is a collection kept behind the ArticleIncludes.Categories flag.
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.ShoppingList!);
                if (includes?.HasFlag(ArticleIncludes.Categories) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(ac => ac.Category);
                return query;
            });
            e.Related(x => x.Categories);
        });
}
