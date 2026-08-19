using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Reservations;

public static class ReservationServiceConfiguration
{
    // complex: 2/2 complex slots - typed StartTime sorting for the timeline/calendar view
    public static EntityServiceCollection<RoomPlannerDbContext> AddReservations(this IEntityServiceCollection<RoomPlannerDbContext> services)
        => services.For<Reservation, ReservationSearchObject, ReservationSortBy, EntityIncludes>(e =>
        {
            e.AddFilter<ReservationQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                ReservationSortBy.StartTime => query.OrderOrThenBy(x => x.StartTime),
                ReservationSortBy.StartTimeDesc => query.OrderOrThenByDescending(x => x.StartTime),
                ReservationSortBy.Created => query.OrderOrThenBy(x => x.Created),
                ReservationSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenBy(x => x.StartTime)
            });
            // Organizer (to-one), Rooms and Attendees are all needed on every calendar/timeline row,
            // so all three load unconditionally. Two collection navigations -> AsSplitQuery.
            e.Includes((query, _) => query
                .Include(x => x.Organizer!)
                .Include(x => x.Rooms!).ThenInclude(rr => rr.Room!).ThenInclude(r => r.Floor!).ThenInclude(f => f.Building!)
                .Include(x => x.Attendees!).ThenInclude(a => a.Employee!)
                .AsSplitQuery());
            e.Related(x => x.Rooms);
            e.Related(x => x.Attendees);
            e.AddTransient<IReservationService, ReservationManager>();
            e.UseEntityService<ReservationManager>();
        });
}
