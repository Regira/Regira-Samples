using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using ShopMate.Api.Data;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    // Entity budget tally (free tier = 5 simple + 2 complex):
    //   ShoppingList        simple   1/5 simple
    //   Category            simple   2/5 simple
    //   Article              complex  1/2 complex
    //   ArticleCategory     owned join via e.Related() on Article  - no slot
    //   RelatedCategory     owned join via e.Related() on Category - no slot
    // -> 2 simple / 1 complex -> fits free tier
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<ShopMateDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddCategories()
            .AddShoppingLists()
            .AddArticles();
}
