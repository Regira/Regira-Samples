using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using Webshop.API.Data;

namespace Webshop.API.Entities.Products;

public static class ProductServiceConfiguration
{
    public static EntityServiceCollection<WebshopDbContext> AddProducts(
        this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>(e =>
        {
            e.UseMapping<ProductDto, ProductInputDto>();
            e.AddMapping<ProductCategoryDto, ProductCategoryDto>();
            e.AddMapping<ProductCategoryInputDto, ProductCategory>();

            e.AddFilter<ProductQueryBuilder>();

            e.SortBy((query, sortBy) =>
            {
                if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type)
                    && query is IOrderedQueryable<Product> sorted)
                    return sortBy switch
                    {
                        ProductSortBy.Title => sorted.ThenBy(x => x.Title),
                        ProductSortBy.TitleDesc => sorted.ThenByDescending(x => x.Title),
                        ProductSortBy.Price => sorted.ThenBy(x => x.Price),
                        ProductSortBy.PriceDesc => sorted.ThenByDescending(x => x.Price),
                        ProductSortBy.Newest => sorted.ThenByDescending(x => x.Created),
                        _ => sorted.ThenBy(x => x.Title)
                    };
                return sortBy switch
                {
                    ProductSortBy.Title => query.OrderBy(x => x.Title),
                    ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
                    ProductSortBy.Price => query.OrderBy(x => x.Price),
                    ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
                    ProductSortBy.Newest => query.OrderByDescending(x => x.Created),
                    _ => query.OrderBy(x => x.Title)
                };
            });

            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(EntityIncludes.All) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(pc => pc.Category);
                return query;
            });

            e.Related(x => x.Categories);
        });
}
