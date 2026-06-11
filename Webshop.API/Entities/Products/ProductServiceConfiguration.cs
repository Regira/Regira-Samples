using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models;
using Webshop.API.Data;

namespace Webshop.API.Entities.Products;

public static class ProductServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddProducts(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>(e =>
        {
            e.AddFilter<ProductQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                ProductSortBy.Title => query.OrderBy(x => x.Title),
                ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
                ProductSortBy.Price => query.OrderBy(x => x.Price),
                ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
                ProductSortBy.Newest => query.OrderByDescending(x => x.Created),
                ProductSortBy.Oldest => query.OrderBy(x => x.Created),
                _ => query.OrderBy(x => x.Title)
            });
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(EntityIncludes.All) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(pc => pc.Category);
                return query;
            });
            e.Related(x => x.Categories);
        });
        return services;
    }
}
