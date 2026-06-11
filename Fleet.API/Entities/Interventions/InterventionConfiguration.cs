using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

namespace Fleet.API.Entities.Interventions;

public static class InterventionConfiguration
{
    public static IEntityServiceCollection<FleetDbContext> AddInterventions(this IEntityServiceCollection<FleetDbContext> services)
    {
        // Complex registration: rich filtering over all foreign keys + status/date/cost, typed sorting and includes.
        services.For<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes>(e =>
        {
            e.AddFilter<InterventionQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                InterventionSortBy.ScheduledDate => query.OrderBy(x => x.ScheduledDate),
                InterventionSortBy.ScheduledDateDesc => query.OrderByDescending(x => x.ScheduledDate),
                InterventionSortBy.Cost => query.OrderBy(x => x.Cost),
                InterventionSortBy.CostDesc => query.OrderByDescending(x => x.Cost),
                InterventionSortBy.Status => query.OrderBy(x => x.Status),
                _ => query.OrderByDescending(x => x.ScheduledDate)
            });
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(InterventionIncludes.Vehicle) == true)
                    query = query.Include(x => x.Vehicle!);
                if (includes?.HasFlag(InterventionIncludes.InterventionType) == true)
                    query = query.Include(x => x.InterventionType!);
                if (includes?.HasFlag(InterventionIncludes.Supplier) == true)
                    query = query.Include(x => x.Supplier!);
                if (includes?.HasFlag(InterventionIncludes.Invoice) == true)
                    query = query.Include(x => x.Invoice!);
                return query;
            });
        });
        return services;
    }
}
