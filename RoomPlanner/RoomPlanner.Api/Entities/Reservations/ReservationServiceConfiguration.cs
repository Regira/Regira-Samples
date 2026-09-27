using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Reservations;

public static class ReservationServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddReservations(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Reservation, ReservationSearchObject, ReservationSortBy, ReservationIncludes>(e =>
        {
            e.AddFilter<ReservationQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                ReservationSortBy.StartDesc => query.OrderOrThenByDescending(x => x.Start),
                ReservationSortBy.Title => query.OrderOrThenBy(x => x.Title),
                ReservationSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenBy(x => x.Start),
            });
            e.Includes((query, includes) =>
            {
                // the organizer is shown on every row -> unconditional to-one include
                query = query.Include(x => x.Organizer);
                if (includes?.HasFlag(ReservationIncludes.Rooms) == true)
                    query = query.Include(x => x.Rooms!).ThenInclude(r => r.Room!).ThenInclude(r => r.Floor!).ThenInclude(f => f.Building);
                if (includes?.HasFlag(ReservationIncludes.Attendees) == true)
                    query = query.Include(x => x.Attendees!).ThenInclude(a => a.Employee);
                // two collections on Details -> avoid the cartesian explosion
                return query.AsSplitQuery();
            });
            // validation + server-owned state first, then the owned-collection syncs
            e.AddPrepper<ReservationPrepper>();
            e.Related(x => x.Rooms);
            e.Related(x => x.Attendees);
        });
}
