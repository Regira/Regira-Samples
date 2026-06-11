using Fleet.API.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

namespace Fleet.API.Entities.Invoices;

public static class InvoiceConfiguration
{
    public static IEntityServiceCollection<FleetDbContext> AddInvoices(this IEntityServiceCollection<FleetDbContext> services)
    {
        // Simple registration: default SearchObject (Q full-text search on invoice number).
        services.For<Invoice>(e =>
        {
            e.SortBy(query => query.OrderByDescending(x => x.InvoiceDate));
            e.Includes((query, _) => query.Include(x => x.Supplier!));
        });
        return services;
    }
}
