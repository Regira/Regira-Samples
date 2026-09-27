using Regira.Entities.QueryBuilders.Abstractions;

namespace ShopMate.Api.Entities.Articles;

public class ArticleQueryBuilder : FilteredQueryBuilderBase<Article, int, ArticleSearchObject>
{
    public override IQueryable<Article> Build(IQueryable<Article> query, ArticleSearchObject? so)
    {
        if (so == null) return query;
        if (so.ShoppingListId?.Any() == true)
            query = query.Where(x => so.ShoppingListId.Contains(x.ShoppingListId));
        if (so.ShopperId?.Any() == true)
            query = query.Where(x => so.ShopperId.Contains(x.ShoppingList!.ShopperId));
        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(ac => so.CategoryId.Contains(ac.CategoryId)));
        if (so.IsActive.HasValue)
            query = query.Where(x => x.IsActive == so.IsActive.Value);
        return query;
    }
}
