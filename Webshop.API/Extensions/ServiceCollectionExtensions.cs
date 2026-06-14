using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.Mapping.Mapster;
using Webshop.API.Data;
using Webshop.API.Entities.Categories;
using Webshop.API.Entities.Customers;
using Webshop.API.Entities.Orders;
using Webshop.API.Entities.Products;

namespace Webshop.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<WebshopDbContext>((sp, options) =>
            options.UseSqlite(configuration.GetConnectionString("Default"))
                .AddPrimerInterceptors(sp)
                .AddNormalizerInterceptors(sp)
                .AddAutoTruncateInterceptors());

        services
            .UseEntities<WebshopDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.DefaultPageSize = 50;
                options.MaxPageSize = 200;
            })
            .AddCategories()
            .AddProducts()
            .AddCustomers()
            .AddOrders();

        return services;
    }
}
