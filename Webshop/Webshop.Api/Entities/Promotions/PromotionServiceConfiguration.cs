using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Promotions;

public static class PromotionServiceConfiguration
{
    // simple registration with a custom SearchObject (1 simple slot)
    public static EntityServiceCollection<WebshopDbContext> AddPromotions(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Promotion, int, PromotionSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.Live == true)
                    query = query.Where(x => x.IsActive).FilterIsActiveOn(DateTime.UtcNow);
                if (!string.IsNullOrWhiteSpace(so?.Q))
                {
                    var q = $"%{so.Q.Trim()}%";
                    query = query.Where(x => EF.Functions.Like(x.Title, q) || EF.Functions.Like(x.Subtitle, q) || EF.Functions.Like(x.Badge, q));
                }
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.SortOrder).ThenByDescending(x => x.Created));
        });
}
