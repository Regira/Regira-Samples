using Fleet.API.Data;
using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace Fleet.API.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFleetEntityServices(this IServiceCollection services)
        => services
            .UseEntities<FleetDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddInterventionTypes()
            .AddSuppliers()
            .AddVehicles()
            .AddInvoices()
            .AddInterventions()
            .GetServices<FleetDbContext>();
}
