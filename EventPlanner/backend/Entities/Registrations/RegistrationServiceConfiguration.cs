using EventPlanner.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace EventPlanner.Api.Entities.Registrations;

public static class RegistrationServiceConfiguration
{
    // Complex registration (typed sort + includes) — 2/2 complex budget slot
    public static EntityServiceCollection<EventPlannerDbContext> AddRegistrations(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Registration, RegistrationSearchObject, RegistrationSortBy, RegistrationIncludes>(e =>
        {
            e.AddFilter<RegistrationQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                RegistrationSortBy.Created => query.OrderOrThenBy(x => x.Created),
                RegistrationSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });
            // Employee + Event are to-one references shown on every list row: unconditional.
            // SelectedSessions is a collection: gated behind the flag, split query alongside Employee/Event.
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Employee!).Include(x => x.Event!);
                if (includes?.HasFlag(RegistrationIncludes.SelectedSessions) == true)
                    query = query.Include(x => x.SelectedSessions!).ThenInclude(ss => ss.Session!).AsSplitQuery();
                return query;
            });
            e.Related(x => x.SelectedSessions);
        });
}
