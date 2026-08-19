using Fleet.Api.Data;
using Fleet.Api.Entities.Invoices;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Preppers.Abstractions;

namespace Fleet.Api.Entities.Interventions;

// Keeps Invoice.TotalAmount in sync with the interventions that point at it via the optional
// Intervention.InvoiceId FK (aggregate over a non-owned child collection — the invoice owns no
// write path for this collection, so the aggregate is recomputed from persisted rows here).
public class InterventionInvoiceTotalPrepper(FleetDbContext dbContext) : EntityPrepperBase<Intervention>
{
    public override async Task Prepare(Intervention modified, Intervention? original, CancellationToken token = default)
    {
        var affectedInvoiceIds = new HashSet<int>();
        if (original?.InvoiceId != null) affectedInvoiceIds.Add(original.InvoiceId.Value);
        if (modified.InvoiceId != null) affectedInvoiceIds.Add(modified.InvoiceId.Value);

        foreach (var invoiceId in affectedInvoiceIds)
        {
            var invoice = await dbContext.Set<Invoice>().FindAsync([invoiceId], token);
            if (invoice == null) continue;

            var total = await dbContext.Set<Intervention>().AsNoTracking()
                .Where(i => i.InvoiceId == invoiceId && i.Id != modified.Id)
                .SumAsync(i => i.Cost, token);
            if (modified.InvoiceId == invoiceId) total += modified.Cost;

            invoice.TotalAmount = total;
        }
    }
}
