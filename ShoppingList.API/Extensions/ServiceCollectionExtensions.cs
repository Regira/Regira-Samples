using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using Regira.Licensing.DependencyInjection;
using ShoppingListApi.Data;
using ShoppingListApi.Entities.Articles;
using ShoppingListApi.Entities.Categories;
using ShoppingListApi.Entities.Lists;
using ShoppingListApi.Entities.Shoppers;

namespace ShoppingListApi.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Regira entity services for every domain entity plus the granular
    /// shopping-list item service.
    /// </summary>
    public static IServiceCollection AddEntityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the Regira license once before any module setup.
        // Without a key the free tier applies (5 simple / 2 complex registrations) — which this
        // project stays within: Category + Article are complex; Shopper + ShoppingList are simple.
        services.UseRegira(configuration);

        services
            .UseEntities<ShoppingListDbContext>(options =>
            {
                options.UseDefaults();          // primers, global filters (incl. Q search), normalizers
                options.UseMapsterMapping();    // DTO mapping
                options.DefaultPageSize = 50;   // page List/Search endpoints by default
                options.MaxPageSize = 200;
            })
            .AddCategories()   // complex
            .AddArticles()     // complex
            .AddShoppers()     // simple
            .AddShoppingLists(); // simple

        // Granular activate/deactivate operations on individual list items.
        services.AddScoped<IShoppingListItemService, ShoppingListItemService>();

        return services;
    }
}
