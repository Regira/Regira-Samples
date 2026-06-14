using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.API.Entities.Vehicles;

public static class VehicleServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddVehicles(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Vehicle, int, VehicleSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.LicensePlate));
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (!string.IsNullOrWhiteSpace(so.LicensePlate))
                    query = query.Where(x => x.LicensePlate.Contains(so.LicensePlate));
                if (!string.IsNullOrWhiteSpace(so.Brand))
                    query = query.Where(x => x.Brand.Contains(so.Brand));
                if (so.VehicleType?.Any() == true)
                    query = query.Where(x => so.VehicleType.Contains(x.VehicleType));
                if (so.MinMileage.HasValue)
                    query = query.Where(x => x.Mileage >= so.MinMileage.Value);
                if (so.MaxMileage.HasValue)
                    query = query.Where(x => x.Mileage <= so.MaxMileage.Value);
                if (so.InterventionTypeId?.Any() == true)
                    query = query.Where(x => x.AllowedInterventionTypes!.Any(a => so.InterventionTypeId.Contains(a.InterventionTypeId)));
                return query;
            });
            // Always eager-load the allowed intervention types (single, cheap joins).
            e.Includes((query, _) => query
                .Include(x => x.AllowedInterventionTypes!)
                .ThenInclude(a => a.InterventionType));

            // Output projection of the nested join collection.
            e.AddMapping<VehicleInterventionTypeDto, VehicleInterventionTypeDto>();
            // Input collection written back through Related().
            e.AddMapping<VehicleInterventionTypeInputDto, VehicleInterventionType>();
            e.Related<VehicleInterventionType>(x => x.AllowedInterventionTypes);
        });
}
