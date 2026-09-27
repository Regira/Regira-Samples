using Fleet.Api.Data;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace Fleet.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Free-tier budget (5 simple + 2 complex):
    // | Entity                   | Classification                                   | Tally        |
    // |--------------------------|--------------------------------------------------|--------------|
    // | InterventionType         | simple  For<T, int, SearchObject>                | 1/5 simple   |
    // | Supplier                 | simple  For<T, int, SearchObject>                | 2/5 simple   |
    // | SupplierInterventionType | owned join via Supplier.Related() - no slot      | -            |
    // | Invoice                  | simple  For<T, int, SearchObject>                | 3/5 simple   |
    // | Vehicle                  | complex (typed sort, fleet data table)           | 1/2 complex  |
    // | Intervention             | complex (typed sort + Lines include)             | 2/2 complex  |
    // | InterventionLine         | owned child via Intervention.Related() - no slot | -            |
    // => 3 simple / 2 complex -> fits free
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<FleetDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddInterventionTypes()
            .AddSuppliers()
            .AddInvoices()
            .AddVehicles()
            .AddInterventions();
}
