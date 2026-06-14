using Regira.Entities.QueryBuilders.Abstractions;

namespace ShoppingListApi.Entities.Articles;

/// <summary>
/// Translates <see cref="ArticleSearchObject"/> category/brand filters to SQL. Full-text <c>Q</c>
/// search is handled globally by the framework's normalized-content filter.
/// </summary>
public class ArticleQueryBuilder : FilteredQueryBuilderBase<Article, ArticleSearchObject>
{
    public override IQueryable<Article> Build(IQueryable<Article> query, ArticleSearchObject? so)
    {
        if (so == null) return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(ac => so.CategoryId.Contains(ac.CategoryId)));

        if (!string.IsNullOrWhiteSpace(so.Brand))
            query = query.Where(x => x.Brand != null && x.Brand.Contains(so.Brand));

        return query;
    }
}
