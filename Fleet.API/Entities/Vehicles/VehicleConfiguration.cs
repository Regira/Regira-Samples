using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Models;

namespace Fleet.API.Entities.Vehicles;

public static class VehicleConfiguration
{
    public static IEntityServiceCollection<FleetDbContext> AddVehicles(this IEntityServiceCollection<FleetDbContext> services)
    {
        // Complex registration: custom SearchObject + Includes flags.
        services.For<Vehicle, VehicleSearchObject, EntitySortBy, VehicleIncludes>(e =>
        {
            e.AddFilter<VehicleQueryBuilder>();
            e.SortBy((query, _) => query.OrderBy(x => x.LicensePlate));
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(VehicleIncludes.AllowedInterventionTypes) == true)
                    query = query.Include(x => x.AllowedInterventionTypes!).ThenInclude(t => t.InterventionType);
                if (includes?.HasFlag(VehicleIncludes.Interventions) == true)
                    query = query.Include(x => x.Interventions!);
                return query;
            });
            // The allowed-types join collection is owned by the vehicle and synced via Related().
            e.Related(x => x.AllowedInterventionTypes);
        });
        return services;
    }
}
