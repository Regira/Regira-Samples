using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using Webshop.Api.Data;
using Webshop.Api.Entities.Brands;
using Webshop.Api.Entities.Categories;
using Webshop.Api.Entities.Orders;
using Webshop.Api.Entities.Products;
using Webshop.Api.Entities.Promotions;

namespace Webshop.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Free-tier budget (5 simple + 2 complex):
    // | Entity    | Classification                                   | Tally        |
    // |-----------|--------------------------------------------------|--------------|
    // | Category  | simple                                           | 1/5 simple   |
    // | Brand     | simple                                           | 2/5 simple   |
    // | Promotion | simple (custom SearchObject)                     | 3/5 simple   |
    // | Product   | complex (typed ProductSortBy)                    | 1/2 complex  |
    // | Order     | complex (typed OrderSortBy + OrderIncludes)      | 2/2 complex  |
    // | OrderLine | owned child via e.Related() on Order - no slot   | -            |
    // => 3 simple / 2 complex -> fits free
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<WebshopDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.DefaultPageSize = 24;
            })
            .AddCategories()
            .AddBrands()
            .AddPromotions()
            .AddProducts()
            .AddOrders();
}
