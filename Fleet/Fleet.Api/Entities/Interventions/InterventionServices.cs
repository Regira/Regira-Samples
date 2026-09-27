using Fleet.Api.Data;
using Fleet.Api.Entities.Invoices;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Normalizing.Abstractions;
using Regira.Entities.Preppers.Abstractions;
using Regira.Entities.Reactors.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Normalizing.Abstractions;

namespace Fleet.Api.Entities.Interventions;

/// <summary>
/// Validates the intervention (FKs, supplier capabilities, invoice consistency) and derives
/// <see cref="Intervention.TotalCost"/> from the lines. Runs inside Add/Modify/Save, before SaveChanges.
/// </summary>
public class InterventionPrepper(FleetDbContext db) : EntityPrepperBase<Intervention>
{
    public override async Task Prepare(Intervention modified, Intervention? original, CancellationToken token = default)
    {
        var errors = new Dictionary<string, string>();

        if (!await db.Vehicles.AnyAsync(v => v.Id == modified.VehicleId, token))
            errors[nameof(Intervention.VehicleId)] = "Unknown vehicle.";
        if (!await db.Suppliers.AnyAsync(s => s.Id == modified.SupplierId, token))
            errors[nameof(Intervention.SupplierId)] = "Unknown supplier.";

        // completion date only makes sense for completed interventions
        if (modified.Status == InterventionStatus.Completed)
            modified.CompletedDate ??= new[] { modified.ScheduledDate, DateOnly.FromDateTime(DateTime.UtcNow) }.Min();
        else
            modified.CompletedDate = null;

        // three-way on the owned collection: null = not sent (use the persisted lines), otherwise the new set
        List<int> typeIds;
        if (modified.Lines != null)
        {
            typeIds = modified.Lines.Select(l => l.InterventionTypeId).ToList();
            if (modified.Lines.Any(l => l.Cost < 0))
                errors[nameof(Intervention.Lines)] = "Costs cannot be negative.";
            modified.TotalCost = modified.Lines.Sum(l => l.Cost);
        }
        else if (modified.Id > 0)
        {
            var persisted = await db.Set<InterventionLine>().AsNoTracking()
                .Where(l => l.InterventionId == modified.Id)
                .Select(l => new { l.InterventionTypeId, l.Cost })
                .ToListAsync(token);
            typeIds = persisted.Select(l => l.InterventionTypeId).ToList();
            modified.TotalCost = persisted.Sum(l => l.Cost);
        }
        else
        {
            typeIds = [];
            modified.TotalCost = 0;
        }

        if (typeIds.Count == 0)
            errors[nameof(Intervention.Lines)] = "Add at least one intervention type.";
        else if (typeIds.Distinct().Count() != typeIds.Count)
            errors[nameof(Intervention.Lines)] = "Each intervention type can only be listed once.";
        else if (!errors.ContainsKey(nameof(Intervention.SupplierId)))
        {
            // suppliers may only perform the intervention types they are assigned
            var capable = await db.Set<Suppliers.SupplierInterventionType>().AsNoTracking()
                .Where(x => x.SupplierId == modified.SupplierId)
                .Select(x => x.InterventionTypeId)
                .ToListAsync(token);
            var missing = typeIds.Except(capable).ToList();
            if (missing.Count > 0)
            {
                var titles = await db.InterventionTypes.AsNoTracking()
                    .Where(t => missing.Contains(t.Id)).Select(t => t.Title).ToListAsync(token);
                errors[nameof(Intervention.Lines)] = $"The supplier cannot perform: {string.Join(", ", titles)}.";
            }
        }

        if (modified.InvoiceId.HasValue)
        {
            var invoice = await db.Invoices.AsNoTracking()
                .Where(i => i.Id == modified.InvoiceId)
                .Select(i => new { i.SupplierId })
                .FirstOrDefaultAsync(token);
            if (invoice == null)
                errors[nameof(Intervention.InvoiceId)] = "Unknown invoice.";
            else if (invoice.SupplierId != modified.SupplierId)
                errors[nameof(Intervention.InvoiceId)] = "The invoice belongs to another supplier.";
            else if (modified.Status != InterventionStatus.Completed)
                errors[nameof(Intervention.InvoiceId)] = "Only completed interventions can be invoiced.";
        }

        if (errors.Count > 0)
            throw new EntityInputException<Intervention>("Invalid intervention") { Item = modified, InputErrors = errors };
    }
}

/// <summary>
/// Folds vehicle plate/make/model, supplier name, type titles and description into NormalizedContent,
/// so one ?q= search hits all of them. Composes the whole value (never appends).
/// </summary>
public class InterventionNormalizer(FleetDbContext db, INormalizer normalizer) : EntityNormalizerBase<Intervention>
{
    public override Task HandleNormalize(Intervention item, CancellationToken token = default)
        => HandleNormalizeMany([item], token);

    public override async Task HandleNormalizeMany(IEnumerable<Intervention> items, CancellationToken token = default)
    {
        var list = items.ToList();
        if (list.Count == 0) return;
        var vehicleIds = list.Select(x => x.VehicleId).Distinct().ToList();
        var supplierIds = list.Select(x => x.SupplierId).Distinct().ToList();
        var vehicles = await db.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id))
            .Select(v => new { v.Id, v.LicensePlate, v.Make, v.Model }).ToDictionaryAsync(v => v.Id, token);
        var suppliers = await db.Suppliers.AsNoTracking().Where(s => supplierIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Title }).ToDictionaryAsync(s => s.Id, s => s.Title, token);
        var types = await db.InterventionTypes.AsNoTracking()
            .Select(t => new { t.Id, t.Title }).ToDictionaryAsync(t => t.Id, t => t.Title, token);

        foreach (var item in list)
        {
            var parts = new List<string?> { item.Description };
            if (vehicles.TryGetValue(item.VehicleId, out var v)) parts.AddRange([v.LicensePlate, v.Make, v.Model]);
            if (suppliers.TryGetValue(item.SupplierId, out var s)) parts.Add(s);
            // in-memory lines (same-save safe); absent on a PATCH without lines -> keep previous type titles out
            if (item.Lines != null)
                parts.AddRange(item.Lines.Select(l => types.GetValueOrDefault(l.InterventionTypeId)));
            var text = string.Join(' ', parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            var normalized = normalizer.Normalize(text);
            item.NormalizedContent = normalized?.Length > 1024 ? normalized[..1024] : normalized;
        }
    }
}

/// <summary>
/// Invoice totals are derived from the interventions that point at the invoice (the intervention owns
/// that write), so any committed change of an intervention's invoice or cost re-saves the affected invoice(s).
/// </summary>
public class InterventionInvoiceReactor(FleetDbContext db, IEntityService<Invoice, int> invoiceService) : EntityReactorBase<Intervention>
{
    public override bool CanReact(IEntityChange<Intervention> change) => change.Kind switch
    {
        EntityChangeKind.Added => change.Entity.InvoiceId.HasValue,
        EntityChangeKind.Deleted => change.Entity.InvoiceId.HasValue,
        _ => change.HasChanged(x => x.InvoiceId) || (change.Entity.InvoiceId.HasValue && change.HasChanged(x => x.TotalCost))
    };

    public override async Task React(IEntityChange<Intervention> change, CancellationToken token = default)
    {
        var invoiceIds = new[] { change.Entity.InvoiceId, change.Original?.InvoiceId }
            .Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var invoices = await db.Invoices.AsNoTracking().Where(i => invoiceIds.Contains(i.Id)).ToListAsync(token);
        foreach (var invoice in invoices)
            await invoiceService.Modify(invoice, token);   // re-runs the invoice prepper (amount recompute)
        if (invoices.Count > 0)
            await invoiceService.SaveChanges(token);
    }
}
