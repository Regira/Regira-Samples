using Fleet.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace Fleet.Api.Entities.Invoices;

public static class InvoiceServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddInvoices(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<Invoice, int, InvoiceSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.SupplierId?.Any() == true) query = query.Where(x => so.SupplierId.Contains(x.SupplierId));
                if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (so.IsOverdue.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    query = so.IsOverdue.Value
                        ? query.Where(x => x.Status != InvoiceStatus.Paid && x.DueDate < today)
                        : query.Where(x => x.Status == InvoiceStatus.Paid || x.DueDate >= today);
                }
                if (so.MinDate.HasValue) query = query.Where(x => x.InvoiceDate >= so.MinDate);
                if (so.MaxDate.HasValue) query = query.Where(x => x.InvoiceDate <= so.MaxDate);
                return query;
            });
            e.SortBy(query => query.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.Id));
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Supplier);   // shown on every row
                if (includes?.HasFlag(EntityIncludes.All) == true)   // Details only (simple entity)
                    query = query
                        .Include(x => x.Interventions!).ThenInclude(i => i.Vehicle)
                        .Include(x => x.Interventions!).ThenInclude(i => i.Lines!).ThenInclude(l => l.InterventionType)
                        .AsSplitQuery();
                return query;
            });
            // Amounts are derived from the persisted interventions (non-owned children, see
            // entities.patterns -> Aggregates over a non-owned child collection).
            e.Prepare(async (invoice, db) =>
            {
                if (!await db.Suppliers.AnyAsync(s => s.Id == invoice.SupplierId))
                    throw new EntityInputException<Invoice>("Invalid invoice")
                    {
                        Item = invoice,
                        InputErrors = new Dictionary<string, string> { [nameof(Invoice.SupplierId)] = "Unknown supplier." }
                    };
                if (invoice.DueDate == default || invoice.DueDate < invoice.InvoiceDate)
                    invoice.DueDate = invoice.InvoiceDate.AddDays(30);
                if (invoice.Status == InvoiceStatus.Paid)
                    invoice.PaidDate ??= DateOnly.FromDateTime(DateTime.UtcNow);
                else
                    invoice.PaidDate = null;

                var costs = invoice.Id > 0
                    ? await db.Interventions.AsNoTracking()
                        .Where(i => i.InvoiceId == invoice.Id)
                        .Select(i => i.TotalCost)
                        .ToListAsync()
                    : [];
                invoice.SubTotal = costs.Sum();
                invoice.VatAmount = Math.Round(invoice.SubTotal * invoice.VatRate / 100m, 2);
                invoice.TotalAmount = invoice.SubTotal + invoice.VatAmount;
            });
        });
}
