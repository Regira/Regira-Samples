using AssetHub.Api.Data;
using AssetHub.Api.Services;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;

namespace AssetHub.Api.Entities.Assets;

/// <summary>
/// - guards the workflow-owned fields (current holder) against ordinary PUT / PATCH
/// - mints the asset tag when left empty
/// - validates references and owned rows (400 instead of a 409 / 500)
/// </summary>
public class AssetPrepper(AppDbContext dbContext, WorkflowContext workflow) : EntityPrepperBase<Asset>
{
    public override async Task Prepare(Asset modified, Asset? original, CancellationToken token = default)
    {
        if (!workflow.IsTrustedWriter)
        {
            modified.CurrentEmployeeId = original?.CurrentEmployeeId;
            modified.AssignedOn = original?.AssignedOn;
        }

        if (string.IsNullOrWhiteSpace(modified.Code))
        {
            modified.Code = original?.Code;
            if (string.IsNullOrWhiteSpace(modified.Code))
            {
                var next = (await dbContext.Assets.AsNoTracking().MaxAsync(x => (int?)x.Id, token) ?? 0) + 1;
                var code = $"AST-{next:D5}";
                while (await dbContext.Assets.AsNoTracking().AnyAsync(x => x.Code == code, token))
                    code = $"AST-{++next:D5}";
                modified.Code = code;
            }
        }
        modified.Code = modified.Code!.Trim().ToUpperInvariant();

        var errors = new Dictionary<string, string>();
        if (!await dbContext.Categories.AnyAsync(x => x.Id == modified.CategoryId, token))
            errors[nameof(Asset.CategoryId)] = "Unknown category.";
        if (!await dbContext.AssetStatuses.AnyAsync(x => x.Id == modified.StatusId, token))
            errors[nameof(Asset.StatusId)] = "Unknown status.";
        if (modified.LocationId.HasValue && !await dbContext.Locations.AnyAsync(x => x.Id == modified.LocationId, token))
            errors[nameof(Asset.LocationId)] = "Unknown location.";
        if (modified.SupplierId.HasValue && !await dbContext.Suppliers.AnyAsync(x => x.Id == modified.SupplierId, token))
            errors[nameof(Asset.SupplierId)] = "Unknown supplier.";
        if (modified.Warranties?.Any(w => w.EndDate < w.StartDate) == true)
            errors[nameof(Asset.Warranties)] = "A warranty cannot end before it starts.";
        if (modified.MaintenanceRecords?.Any(m => m.NextDueDate.HasValue && m.NextDueDate < m.Date) == true)
            errors[nameof(Asset.MaintenanceRecords)] = "The next due date cannot precede the maintenance date.";

        if (errors.Count > 0)
        {
            var ex = new EntityInputException<Asset>("Saving asset failed");
            foreach (var (key, message) in errors)
                ex.InputErrors[key] = message;
            throw ex;
        }
    }
}
