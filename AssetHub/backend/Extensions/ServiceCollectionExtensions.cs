using AssetHub.Api.Data;
using AssetHub.Api.Entities.AssetAssignments;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace AssetHub.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Budget tally: 5 simple (Category, AssetStatus, Location, Supplier, Employee)
    //             + 2 complex (Asset, AssetAssignment)  -> 7/7, fits the free tier exactly.
    // Owned children (no slot): AssetAttachment, AssetWarranty, AssetMaintenanceRecord (via Asset.Related()).
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<AppDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddCategories()
            .AddAssetStatuses()
            .AddLocations()
            .AddSuppliers()
            .AddEmployees()
            .AddAssets()
            .AddAssetAssignments();
}
