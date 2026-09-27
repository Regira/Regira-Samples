using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using ShopMate.Api.Data;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.Shoppers;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Free-tier budget (5 simple + 2 complex):
    // | Entity          | Classification                           | Running tally |
    // |-----------------|------------------------------------------|---------------|
    // | Shopper         | simple                                   | 1/5 simple    |
    // | ShoppingList    | simple (+ SearchObject)                  | 2/5 simple    |
    // | Category        | simple (+ SearchObject)                  | 3/5 simple    |
    // | RelatedCategory | owned join via Category.Related()        | -             |
    // | Article         | complex (typed SortBy + Includes)        | 1/2 complex   |
    // | ArticleCategory | owned join via Article.Related()         | -             |
    // -> 3 simple / 1 complex -> fits free
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<ShopMateDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddShoppers()
            .AddShoppingLists()
            .AddCategories()
            .AddArticles();
}
