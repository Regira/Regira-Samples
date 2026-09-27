using EventPlanner.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace EventPlanner.Api.Entities.Registrations;

public static class RegistrationServiceConfiguration
{
    public static EntityServiceCollection<EventPlannerDbContext> AddRegistrations(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Registration, RegistrationSearchObject, RegistrationSortBy, RegistrationIncludes>(e =>
        {
            e.AddFilter<RegistrationQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                RegistrationSortBy.CreatedAsc => query.OrderOrThenBy(x => x.Created),
                RegistrationSortBy.EventDate => query.OrderOrThenBy(x => x.Event!.StartDate),
                RegistrationSortBy.EventDateDesc => query.OrderOrThenByDescending(x => x.Event!.StartDate),
                RegistrationSortBy.Employee => query.OrderOrThenBy(x => x.User!.LastName).OrderOrThenBy(x => x.User!.FirstName),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Event).Include(x => x.User);
                if (includes?.HasFlag(RegistrationIncludes.Sessions) == true)
                    query = query.Include(x => x.Sessions!).ThenInclude(s => s.Session);
                return query;
            });
            // business rules first, then the owned session-selection sync
            e.AddPrepper<RegistrationPrepper>();
            e.Related(x => x.Sessions);
        });
}
