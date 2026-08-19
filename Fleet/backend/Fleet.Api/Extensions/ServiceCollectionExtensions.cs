using Fleet.Api.Data;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace Fleet.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Entity budget tally (free tier = 5 simple + 2 complex):
    //   Vehicle              simple  1/5
    //   Supplier             simple  2/5
    //   InterventionType     simple  3/5
    //   SupplierInterventionType     owned via e.Related() on Supplier -- no slot
    //   InterventionInterventionType owned via e.Related() on Intervention -- no slot
    //   Intervention          complex 1/2
    //   Invoice                complex 2/2
    // -> 3 simple / 2 complex registered -> fits free tier.
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.AddSingleton<InvoiceCodeGenerator>();

        return services
            .UseEntities<FleetDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddVehicles()
            .AddSuppliers()
            .AddInterventionTypes()
            .AddInterventions()
            .AddInvoices();
    }
}
