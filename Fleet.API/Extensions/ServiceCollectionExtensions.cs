using Fleet.API.Data;
using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using Regira.Licensing.DependencyInjection;

namespace Fleet.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Regira entity services (repositories, query builders, mapping, primers)
    /// for every Fleet entity.
    /// </summary>
    public static IServiceCollection AddEntityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Optional paid license. Without a key the free tier (5 simple / 2 complex) applies automatically.
        var licenseKey = configuration["Regira:LicenseKey"];
        if (!string.IsNullOrWhiteSpace(licenseKey))
            services.UseRegira(licenseKey);

        return services
            .UseEntities<FleetDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.DefaultPageSize = 25;
                options.MaxPageSize = 200;
            })
            // 3 simple registrations
            .AddInterventionTypes()
            .AddVehicles()
            .AddSuppliers()
            // 2 complex registrations
            .AddInvoices()
            .AddInterventions();
    }
}
