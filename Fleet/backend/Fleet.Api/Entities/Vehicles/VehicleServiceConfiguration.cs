using Fleet.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.Api.Entities.Vehicles;

public static class VehicleServiceConfiguration
{
    // Budget: simple (1/5 simple)
    public static EntityServiceCollection<FleetDbContext> AddVehicles(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Vehicle, int, VehicleSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.Type?.Any() == true) query = query.Where(x => so.Type.Contains(x.Type));
                if (so?.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (so?.MinYear != null) query = query.Where(x => x.Year >= so.MinYear.Value);
                if (so?.MaxYear != null) query = query.Where(x => x.Year <= so.MaxYear.Value);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.LicensePlate));
        });
}
