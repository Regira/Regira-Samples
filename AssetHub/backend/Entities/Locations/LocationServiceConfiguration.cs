using AssetHub.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace AssetHub.Api.Entities.Locations;

// Budget: simple 3/5
public static class LocationServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddLocations(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Location, int, LocationSearchObject>(e =>
        {
            e.Filter((query, so) => string.IsNullOrWhiteSpace(so?.Title)
                ? query
                : query.Where(x => x.Title.Contains(so.Title)));
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
