using Regira.Entities.QueryBuilders.Abstractions;

namespace Webshop.Api.Entities.Products;

public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;
        if (so.CategoryId?.Any() == true) query = query.Where(x => so.CategoryId.Contains(x.CategoryId));
        if (so.BrandId?.Any() == true) query = query.Where(x => x.BrandId.HasValue && so.BrandId.Contains(x.BrandId.Value));
        if (so.MinPrice.HasValue) query = query.Where(x => x.Price >= so.MinPrice.Value);
        if (so.MaxPrice.HasValue) query = query.Where(x => x.Price <= so.MaxPrice.Value);
        if (so.MinRating.HasValue) query = query.Where(x => x.Rating >= so.MinRating.Value);
        if (so.OnSale.HasValue)
            query = so.OnSale.Value
                ? query.Where(x => x.CompareAtPrice != null && x.CompareAtPrice > x.Price)
                : query.Where(x => x.CompareAtPrice == null || x.CompareAtPrice <= x.Price);
        if (so.InStock.HasValue) query = so.InStock.Value ? query.Where(x => x.Stock > 0) : query.Where(x => x.Stock <= 0);
        if (so.IsFeatured.HasValue) query = query.Where(x => x.IsFeatured == so.IsFeatured.Value);
        if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive.Value);
        return query;
    }
}
