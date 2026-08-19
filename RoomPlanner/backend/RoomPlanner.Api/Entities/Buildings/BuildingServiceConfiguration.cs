using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Buildings;

public static class BuildingServiceConfiguration
{
    // simple: 1/5 simple slots
    public static EntityServiceCollection<RoomPlannerDbContext> AddBuildings(this IEntityServiceCollection<RoomPlannerDbContext> services)
        => services.For<Building, int, BuildingSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.City))
                {
                    query = query.Where(x => x.City == so.City);
                }
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
