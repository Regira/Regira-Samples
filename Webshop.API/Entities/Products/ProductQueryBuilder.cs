using Regira.Entities.QueryBuilders.Abstractions;

namespace Webshop.API.Entities.Products;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
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
            query = query.Where(p => p.StockQuantity > 0);
        return query;
    }
}
