using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Products;

public static class ProductServiceConfiguration
{
    // Budget: complex registration (1/2 complex) - user-selectable sort (price/title/rating/newest)
    public static EntityServiceCollection<WebshopDbContext> AddProducts(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Product, ProductSearchObject, ProductSortBy, Regira.Entities.Models.EntityIncludes>(e =>
        {
            e.AddFilter<ProductQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                ProductSortBy.Title => query.OrderOrThenBy(x => x.Title),
                ProductSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                ProductSortBy.Price => query.OrderOrThenBy(x => x.Price),
                ProductSortBy.PriceDesc => query.OrderOrThenByDescending(x => x.Price),
                ProductSortBy.Rating => query.OrderOrThenByDescending(x => x.Rating),
                ProductSortBy.Newest => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenByDescending(x => x.IsFeatured).OrderOrThenByDescending(x => x.Created)
            });
            // Category is a to-one shown on every product card - load unconditionally.
            e.Includes((query, _) => query.Include(x => x.Category!));
        });
}
