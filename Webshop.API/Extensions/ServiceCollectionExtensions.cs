using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster;
using Webshop.API.Data;
using Webshop.API.Entities.Categories;
using Webshop.API.Entities.Customers;
using Webshop.API.Entities.Orders;
using Webshop.API.Entities.Products;

namespace Webshop.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .UseEntities<WebshopDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddCategories()
            .AddProducts()
            .AddCustomers()
            .AddOrders()
            .GetServices<WebshopDbContext>();
}
