using Microsoft.EntityFrameworkCore;
using Regira.Entities.Keywords.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace ShopMate.Api.Entities.Articles;

public class ArticleQueryBuilder(IQKeywordHelper qHelper) : FilteredQueryBuilderBase<Article, int, ArticleSearchObject>
{
    public override IQueryable<Article> Build(IQueryable<Article> query, ArticleSearchObject? so)
    {
        if (so == null) return query;

        if (so.ShoppingListId?.Any() == true)
            query = query.Where(x => so.ShoppingListId.Contains(x.ShoppingListId));
        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(ac => so.CategoryId.Contains(ac.CategoryId)));
        if (so.IsActive.HasValue)
            query = query.Where(x => x.IsActive == so.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(so.Q))
        {
            var keywords = qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, keyword.QW));
        }

        return query;
    }
}
