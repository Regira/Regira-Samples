using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using ShopMate.Api.Data;

namespace ShopMate.Api.Entities.Articles;

public static class ArticleServiceConfiguration
{
    // complex registration (1 complex slot); ArticleCategory is an owned join row (no slot)
    public static EntityServiceCollection<ShopMateDbContext> AddArticles(this IEntityServiceCollection<ShopMateDbContext> services)
        => services.For<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes>(e =>
        {
            e.AddFilter<ArticleQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                ArticleSortBy.SortOrder => query.OrderOrThenBy(x => x.SortOrder),
                ArticleSortBy.ActiveFirst => query.OrderOrThenByDescending(x => x.IsActive).ThenBy(x => x.SortOrder),
                ArticleSortBy.Title => query.OrderOrThenBy(x => x.Title),
                ArticleSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                ArticleSortBy.Created => query.OrderOrThenBy(x => x.Created),
                ArticleSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenBy(x => x.ShoppingListId).ThenBy(x => x.SortOrder).ThenBy(x => x.Id)
            });
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(ArticleIncludes.ShoppingList) == true)
                    query = query.Include(x => x.ShoppingList);
                if (includes?.HasFlag(ArticleIncludes.Categories) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(x => x.Category);
                return query;
            });
            // Validate the list FK (400 instead of 409) and mint SortOrder on create (append to the end).
            // [ServerOwned] restores SortOrder from the stored row on update, so only new rows are minted here.
            e.Prepare(async (item, db) =>
            {
                var listExists = await db.ShoppingLists.AnyAsync(x => x.Id == item.ShoppingListId);
                if (!listExists)
                {
                    throw new EntityInputException<Article>("Invalid shopping list")
                    {
                        Item = item,
                        InputErrors = new Dictionary<string, string> { [nameof(Article.ShoppingListId)] = "Shopping list does not exist" }
                    };
                }
                if (item.Id == 0 && item.SortOrder == 0)
                {
                    var max = await db.Articles
                        .Where(x => x.ShoppingListId == item.ShoppingListId)
                        .MaxAsync(x => (int?)x.SortOrder) ?? 0;
                    item.SortOrder = max + 1;
                }
            });
            e.Related(x => x.Categories);
        });
}
