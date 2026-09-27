using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Buildings;

public static class BuildingServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddBuildings(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Building>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
