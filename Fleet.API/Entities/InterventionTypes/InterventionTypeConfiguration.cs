using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

namespace Fleet.API.Entities.InterventionTypes;

public static class InterventionTypeConfiguration
{
    public static IEntityServiceCollection<FleetDbContext> AddInterventionTypes(this IEntityServiceCollection<FleetDbContext> services)
    {
        // Simple registration: default SearchObject (supports Id/Ids/Q full-text search).
        services.For<InterventionType>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
        return services;
    }
}
