using Fleet.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;

namespace Fleet.Api.Entities.Vehicles;

public static class VehicleServiceConfiguration
{
    // complex: typed sorting for the fleet data table
    public static EntityServiceCollection<FleetDbContext> AddVehicles(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Vehicle, VehicleSearchObject, VehicleSortBy, EntityIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.VehicleType?.Any() == true) query = query.Where(x => so.VehicleType.Contains(x.VehicleType));
                if (so.FuelType?.Any() == true) query = query.Where(x => so.FuelType.Contains(x.FuelType));
                if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (!string.IsNullOrWhiteSpace(so.Department)) query = query.Where(x => x.Department == so.Department);
                if (so.ServiceDueBefore.HasValue) query = query.Where(x => x.NextServiceDate != null && x.NextServiceDate <= so.ServiceDueBefore);
                if (so.MinYear.HasValue) query = query.Where(x => x.Year >= so.MinYear);
                if (so.MaxYear.HasValue) query = query.Where(x => x.Year <= so.MaxYear);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                VehicleSortBy.LicensePlate => query.OrderOrThenBy(x => x.LicensePlate),
                VehicleSortBy.LicensePlateDesc => query.OrderOrThenByDescending(x => x.LicensePlate),
                VehicleSortBy.Make => query.OrderOrThenBy(x => x.Make).ThenBy(x => x.Model),
                VehicleSortBy.MakeDesc => query.OrderOrThenByDescending(x => x.Make).ThenByDescending(x => x.Model),
                VehicleSortBy.Year => query.OrderOrThenBy(x => x.Year),
                VehicleSortBy.YearDesc => query.OrderOrThenByDescending(x => x.Year),
                VehicleSortBy.Mileage => query.OrderOrThenBy(x => x.Mileage),
                VehicleSortBy.MileageDesc => query.OrderOrThenByDescending(x => x.Mileage),
                VehicleSortBy.NextServiceDate => query.OrderOrThenBy(x => x.NextServiceDate == null).ThenBy(x => x.NextServiceDate),
                VehicleSortBy.NextServiceDateDesc => query.OrderOrThenByDescending(x => x.NextServiceDate),
                VehicleSortBy.Status => query.OrderOrThenBy(x => x.Status),
                _ => query.OrderOrThenBy(x => x.LicensePlate)
            });
        });
}
