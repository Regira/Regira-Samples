using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster;
using ShoppingList.API.Data;
using ShoppingList.API.Entities.Articles;
using ShoppingList.API.Entities.Categories;
using ShoppingList.API.Entities.Shoppers;
using ShoppingList.API.Entities.ShoppingListItems;
using ShoppingList.API.Entities.ShoppingLists;

namespace ShoppingList.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Regira entity framework (default primers/filters/normalizers + Mapster mapping)
    /// and all shopping-list domain entities.
    /// </summary>
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<ShoppingDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddCategories()
            .AddArticles()
            .AddShoppers()
            .AddShoppingLists()
            .AddShoppingListItems()
            .GetServices<ShoppingDbContext>();
}
