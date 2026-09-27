using EventPlanner.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.Locations;

public static class LocationServiceConfiguration
{
    public static EntityServiceCollection<EventPlannerDbContext> AddLocations(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Location, int, LocationSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.City?.Any() == true) query = query.Where(x => so.City.Contains(x.City!));
                if (so.MinCapacity.HasValue) query = query.Where(x => x.Capacity >= so.MinCapacity);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
