using Microsoft.EntityFrameworkCore;
using Regira.Entities.Keywords.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace ShoppingList.API.Entities.Articles;

/// <summary>
/// Handles all <see cref="Article"/> filtering: category membership, brand, and normalized
/// full-text search (Q) with wildcard support via <see cref="IQKeywordHelper"/>.
/// </summary>
public class ArticleQueryBuilder(IQKeywordHelper qHelper)
    : FilteredQueryBuilderBase<Article, int, ArticleSearchObject>
{
    public override IQueryable<Article> Build(IQueryable<Article> query, ArticleSearchObject? so)
    {
        if (so == null)
            return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(ac => so.CategoryId.Contains(ac.CategoryId)));

        if (so.Brand?.Any() == true)
            query = query.Where(x => x.Brand != null && so.Brand.Contains(x.Brand));

        if (!string.IsNullOrWhiteSpace(so.Q))
        {
            var keywords = qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, keyword.QW));
        }

        return query;
    }
}
