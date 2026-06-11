using Microsoft.EntityFrameworkCore;
using Regira.Entities.Keywords.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace ShoppingList.API.Entities.ShoppingListItems;

/// <summary>
/// Filters <see cref="ShoppingListItem"/> entries by list, article, active state and category,
/// and supports a normalized full-text search (Q) over the related article. Always eager-loads
/// the referenced article so list views are self-contained.
/// </summary>
public class ShoppingListItemQueryBuilder(IQKeywordHelper qHelper)
    : FilteredQueryBuilderBase<ShoppingListItem, int, ShoppingListItemSearchObject>
{
    public override IQueryable<ShoppingListItem> Build(IQueryable<ShoppingListItem> query, ShoppingListItemSearchObject? so)
    {
        query = query.Include(x => x.Article);

        if (so == null)
            return query;

        if (so.ShoppingListId?.Any() == true)
            query = query.Where(x => so.ShoppingListId.Contains(x.ShoppingListId));

        if (so.ArticleId?.Any() == true)
            query = query.Where(x => so.ArticleId.Contains(x.ArticleId));

        if (so.IsActive.HasValue)
            query = query.Where(x => x.IsActive == so.IsActive.Value);

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Article!.Categories!.Any(ac => so.CategoryId.Contains(ac.CategoryId)));

        if (!string.IsNullOrWhiteSpace(so.Q))
        {
            var keywords = qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
                query = query.Where(x => EF.Functions.Like(x.Article!.NormalizedContent, keyword.QW));
        }

        return query;
    }
}
