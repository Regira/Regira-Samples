using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Products;

public static class ProductServiceConfiguration
{
    // complex registration (1 complex slot): typed sorting for the storefront's sort dropdown
    public static EntityServiceCollection<WebshopDbContext> AddProducts(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>(e =>
        {
            e.AddFilter<ProductQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                ProductSortBy.Newest => query.OrderOrThenByDescending(x => x.Created),
                ProductSortBy.PriceAsc => query.OrderOrThenBy(x => x.Price),
                ProductSortBy.PriceDesc => query.OrderOrThenByDescending(x => x.Price),
                ProductSortBy.Rating => query.OrderOrThenByDescending(x => x.Rating).ThenByDescending(x => x.ReviewCount),
                ProductSortBy.Popularity => query.OrderOrThenByDescending(x => x.ReviewCount),
                ProductSortBy.Title => query.OrderOrThenBy(x => x.Title),
                ProductSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                ProductSortBy.Stock => query.OrderOrThenBy(x => x.Stock),
                _ => query.OrderOrThenByDescending(x => x.IsFeatured).ThenByDescending(x => x.ReviewCount)
            });
            // category + brand are shown on every product card -> cheap to-one references, loaded unconditionally
            e.Includes((query, _) => query
                .Include(x => x.Category)
                .Include(x => x.Brand));
        });
}
