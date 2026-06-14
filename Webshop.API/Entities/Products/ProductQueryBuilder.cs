using Microsoft.EntityFrameworkCore;
using Regira.Entities.Keywords.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace Webshop.API.Entities.Products;

public class ProductQueryBuilder(IQKeywordHelper qHelper)
    : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;
        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(pc => so.CategoryId.Contains(pc.CategoryId)));
        if (so.MinPrice.HasValue)
            query = query.Where(p => p.Price >= so.MinPrice.Value);
        if (so.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= so.MaxPrice.Value);
        if (so.InStock == true)
            query = query.Where(p => p.Stock > 0);
        if (!string.IsNullOrWhiteSpace(so.Q))
        {
            var keywords = qHelper.Parse(so.Q);
            foreach (var keyword in keywords)
                query = query.Where(x => EF.Functions.Like(x.NormalizedContent, keyword.QW));
        }
        return query;
    }
}
