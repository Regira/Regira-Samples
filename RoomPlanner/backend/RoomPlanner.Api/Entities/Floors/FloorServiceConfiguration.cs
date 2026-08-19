using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Floors;

public static class FloorServiceConfiguration
{
    // simple: 2/5 simple slots
    public static EntityServiceCollection<RoomPlannerDbContext> AddFloors(this IEntityServiceCollection<RoomPlannerDbContext> services)
        => services.For<Floor, int, FloorSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.BuildingId?.Any() == true)
                {
                    query = query.Where(x => so.BuildingId.Contains(x.BuildingId));
                }
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.BuildingId).ThenBy(x => x.Level));
            e.Includes((query, _) => query.Include(x => x.Building!));
        });
}
