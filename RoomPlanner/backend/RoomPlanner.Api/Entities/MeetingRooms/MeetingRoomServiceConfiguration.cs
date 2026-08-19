using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.MeetingRooms;

public static class MeetingRoomServiceConfiguration
{
    // complex: 1/2 complex slots - typed sorting (capacity/title) drives the browsing experience
    public static EntityServiceCollection<RoomPlannerDbContext> AddMeetingRooms(this IEntityServiceCollection<RoomPlannerDbContext> services)
        => services.For<MeetingRoom, MeetingRoomSearchObject, MeetingRoomSortBy, EntityIncludes>(e =>
        {
            e.AddFilter<MeetingRoomQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                MeetingRoomSortBy.Title => query.OrderOrThenBy(x => x.Title),
                MeetingRoomSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                MeetingRoomSortBy.Capacity => query.OrderOrThenBy(x => x.Capacity),
                MeetingRoomSortBy.CapacityDesc => query.OrderOrThenByDescending(x => x.Capacity),
                _ => query.OrderOrThenBy(x => x.Title)
            });
            // Floor + Building are cheap to-one references shown on every room card - load unconditionally.
            e.Includes((query, _) => query.Include(x => x.Floor!).ThenInclude(f => f.Building!));
        });
}
