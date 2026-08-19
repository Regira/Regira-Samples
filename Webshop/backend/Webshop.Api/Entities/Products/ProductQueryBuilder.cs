using Regira.Entities.QueryBuilders.Abstractions;

namespace Webshop.Api.Entities.Products;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;

        if (!string.IsNullOrWhiteSpace(so.Slug))
            query = query.Where(x => x.Slug == so.Slug);
        if (so.CategoryId?.Any() == true)
            query = query.Where(x => so.CategoryId.Contains(x.CategoryId));
        if (so.Brand?.Any() == true)
            query = query.Where(x => x.Brand != null && so.Brand.Contains(x.Brand));
        if (so.MinPrice.HasValue)
            query = query.Where(x => x.Price >= so.MinPrice.Value);
        if (so.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= so.MaxPrice.Value);
        if (so.InStockOnly == true)
            query = query.Where(x => x.Stock > 0);
        if (so.IsFeatured != null)
            query = query.Where(x => x.IsFeatured == so.IsFeatured);
        if (so.OnSale == true)
            query = query.Where(x => x.CompareAtPrice != null && x.CompareAtPrice > x.Price);

        return query;
    }
}
