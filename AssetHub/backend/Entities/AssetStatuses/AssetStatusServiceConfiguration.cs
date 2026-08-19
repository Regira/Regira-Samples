using AssetHub.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace AssetHub.Api.Entities.AssetStatuses;

// Budget: simple 2/5
public static class AssetStatusServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddAssetStatuses(this IEntityServiceCollection<AppDbContext> services)
        => services.For<AssetStatus, int, AssetStatusSearchObject>(e =>
        {
            e.Filter((query, so) => string.IsNullOrWhiteSpace(so?.Title)
                ? query
                : query.Where(x => x.Title.Contains(so.Title)));
            e.SortBy(query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
        });
}
