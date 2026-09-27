using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Rooms;

public static class RoomServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddRooms(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Room, RoomSearchObject, RoomSortBy, RoomIncludes>(e =>
        {
            e.AddFilter<RoomQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                RoomSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                RoomSortBy.Capacity => query.OrderOrThenBy(x => x.Capacity),
                RoomSortBy.CapacityDesc => query.OrderOrThenByDescending(x => x.Capacity),
                RoomSortBy.Building => query.OrderOrThenBy(x => x.Floor!.Building!.Title).ThenBy(x => x.Floor!.Level).ThenBy(x => x.Title),
                _ => query.OrderOrThenBy(x => x.Title),
            });
            e.Includes((query, includes) =>
            {
                // floor + building are shown on every room card -> unconditional to-one includes
                query = query.Include(x => x.Floor!).ThenInclude(f => f.Building);
                if (includes?.HasFlag(RoomIncludes.Equipment) == true)
                    query = query.Include(x => x.Equipment!).ThenInclude(re => re.Equipment);
                return query;
            });
            e.Related(x => x.Equipment);
            e.Prepare(async (room, db) =>
            {
                var errors = new Dictionary<string, string>();
                if (!await db.Floors.AnyAsync(f => f.Id == room.FloorId))
                    errors[nameof(Room.FloorId)] = "Unknown floor.";
                if (room.Capacity < 1)
                    errors[nameof(Room.Capacity)] = "Capacity must be at least 1.";
                if (room.Equipment != null && room.Equipment.GroupBy(x => x.EquipmentId).Any(g => g.Count() > 1))
                    errors[nameof(Room.Equipment)] = "Each equipment type can only be listed once.";
                if (errors.Count > 0)
                {
                    var ex = new EntityInputException<Room>("Invalid room");
                    foreach (var (key, value) in errors) ex.InputErrors[key] = value;
                    throw ex;
                }
            });
        });
}
