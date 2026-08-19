using Fleet.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace Fleet.Api.Entities.Invoices;

public static class InvoiceServiceConfiguration
{
    // Budget: complex (2/2 complex)
    public static EntityServiceCollection<FleetDbContext> AddInvoices(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Invoice, InvoiceSearchObject, InvoiceSortBy, InvoiceIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Code)) query = query.Where(x => x.Code != null && x.Code.Contains(so.Code));
                if (so?.SupplierId?.Any() == true) query = query.Where(x => so.SupplierId.Contains(x.SupplierId));
                if (so?.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (so?.MinIssueDate != null) query = query.Where(x => x.IssueDate >= so.MinIssueDate.Value);
                if (so?.MaxIssueDate != null) query = query.Where(x => x.IssueDate <= so.MaxIssueDate.Value);
                if (so?.MinDueDate != null) query = query.Where(x => x.DueDate >= so.MinDueDate.Value);
                if (so?.MaxDueDate != null) query = query.Where(x => x.DueDate <= so.MaxDueDate.Value);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                InvoiceSortBy.IssueDate => query.OrderOrThenBy(x => x.IssueDate),
                InvoiceSortBy.IssueDateDesc => query.OrderOrThenByDescending(x => x.IssueDate),
                InvoiceSortBy.DueDate => query.OrderOrThenBy(x => x.DueDate),
                InvoiceSortBy.Status => query.OrderOrThenBy(x => x.Status),
                InvoiceSortBy.TotalAmount => query.OrderOrThenBy(x => x.TotalAmount),
                InvoiceSortBy.TotalAmountDesc => query.OrderOrThenByDescending(x => x.TotalAmount),
                _ => query.OrderOrThenByDescending(x => x.IssueDate)
            });
            // Supplier: cheap to-one reference shown on every row -> unconditional.
            // Interventions: a collection -> flag-gated.
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Supplier!);
                if (includes?.HasFlag(InvoiceIncludes.Interventions) == true)
                    query = query.Include(x => x.Interventions!).ThenInclude(x => x.Vehicle!);
                return query;
            });
            e.AddPrimer<InvoiceCodePrimer>();
        });
}
