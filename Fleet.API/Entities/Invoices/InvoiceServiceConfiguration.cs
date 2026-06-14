using Fleet.API.Data;
using Fleet.API.Entities.Interventions;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.API.Entities.Invoices;

public static class InvoiceServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddInvoices(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Invoice, InvoiceSearchObject, InvoiceSortBy, InvoiceIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (!string.IsNullOrWhiteSpace(so.InvoiceNumber))
                    query = query.Where(x => x.InvoiceNumber.Contains(so.InvoiceNumber));
                if (so.SupplierId?.Any() == true)
                    query = query.Where(x => so.SupplierId.Contains(x.SupplierId));
                if (so.Status?.Any() == true)
                    query = query.Where(x => so.Status.Contains(x.Status));
                if (so.MinIssueDate.HasValue)
                    query = query.Where(x => x.IssueDate >= so.MinIssueDate.Value);
                if (so.MaxIssueDate.HasValue)
                    query = query.Where(x => x.IssueDate <= so.MaxIssueDate.Value);
                if (so.MinTotalAmount.HasValue)
                    query = query.Where(x => x.TotalAmount >= so.MinTotalAmount.Value);
                if (so.MaxTotalAmount.HasValue)
                    query = query.Where(x => x.TotalAmount <= so.MaxTotalAmount.Value);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                InvoiceSortBy.IssueDate => query.OrderBy(x => x.IssueDate),
                InvoiceSortBy.IssueDateDesc => query.OrderByDescending(x => x.IssueDate),
                InvoiceSortBy.DueDate => query.OrderBy(x => x.DueDate),
                InvoiceSortBy.DueDateDesc => query.OrderByDescending(x => x.DueDate),
                InvoiceSortBy.TotalAmount => query.OrderBy(x => x.TotalAmount),
                InvoiceSortBy.TotalAmountDesc => query.OrderByDescending(x => x.TotalAmount),
                _ => query.OrderByDescending(x => x.IssueDate)
            });
            e.Includes((query, includes) =>
            {
                // The supplier is always useful context on an invoice.
                query = query.Include(x => x.Supplier);
                if (includes?.HasFlag(InvoiceIncludes.Interventions) == true)
                    query = query.Include(x => x.Interventions!);
                return query;
            });

            // Interventions are an independent entity, projected (read-only) into the invoice DTO.
            e.AddMapping<InterventionCoreDto, InterventionCoreDto>();
        });
}
