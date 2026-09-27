using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Floors;

public static class FloorServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddFloors(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Floor, int, FloorSearchObject>(e =>
        {
            e.Filter((query, so) => so?.BuildingId?.Any() == true
                ? query.Where(x => so.BuildingId.Contains(x.BuildingId))
                : query);
            e.SortBy(query => query.OrderBy(x => x.Building!.Title).ThenBy(x => x.Level));
            // the building is shown on every list row -> unconditional to-one include
            e.Includes((query, _) => query.Include(x => x.Building));
            e.Prepare(async (floor, db) =>
            {
                if (!await db.Buildings.AnyAsync(b => b.Id == floor.BuildingId))
                    throw new EntityInputException<Floor>("Invalid floor")
                    {
                        InputErrors = { [nameof(Floor.BuildingId)] = "Unknown building." }
                    };
            });
        });
}
