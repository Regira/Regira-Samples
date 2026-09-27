using AssetHub.Api.Data;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace AssetHub.Api.Services;

/// <summary>
/// The only writer of Asset.CurrentEmployeeId / AssignedOn and of the assignment history.
/// Writes through IEntityService so preppers, primers and the Related() sync stay in play.
/// </summary>
public class AssetWorkflowService(
    IEntityService<Asset, AssetSearchObject, AssetSortBy, AssetIncludes> service,
    AppDbContext dbContext,
    WorkflowContext workflow)
{
    public async Task<bool> Assign(int assetId, AssignAssetInput input, CancellationToken token = default)
    {
        var asset = await service.Details(assetId, token);
        if (asset == null) return false;

        var employee = await dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == input.EmployeeId, token);
        if (employee == null) throw InputError(nameof(AssignAssetInput.EmployeeId), "Unknown employee.");
        if (!employee.IsActive) throw InputError(nameof(AssignAssetInput.EmployeeId), "This employee is no longer active.");
        if (asset.CurrentEmployeeId == employee.Id) throw InputError(nameof(AssignAssetInput.EmployeeId), "The asset is already assigned to this employee.");
        if (asset.Status?.Kind is StatusKind.Inactive or StatusKind.Maintenance)
            throw InputError("StatusId", $"An asset with status '{asset.Status.Title}' cannot be assigned.");

        var assignedStatusId = await FindStatusId(StatusKind.Assigned, token)
            ?? throw InputError("StatusId", "No status of kind 'Assigned' is configured.");

        var now = DateTime.UtcNow;
        asset.Assignments ??= [];
        foreach (var open in asset.Assignments.Where(a => a.ReturnedOn == null))
        {
            open.ReturnedOn = now;
            open.ReturnNotes ??= $"Reassigned to {employee.FirstName} {employee.LastName}";
        }
        asset.Assignments.Add(new AssetAssignment
        {
            AssetId = asset.Id,
            EmployeeId = employee.Id,
            AssignedOn = now,
            Notes = input.Notes
        });
        asset.CurrentEmployeeId = employee.Id;
        asset.CurrentEmployee = null;
        asset.AssignedOn = now;
        asset.StatusId = assignedStatusId;
        asset.Status = null;

        await Save(asset, token);
        return true;
    }

    public async Task<bool> Return(int assetId, ReturnAssetInput input, CancellationToken token = default)
    {
        var asset = await service.Details(assetId, token);
        if (asset == null) return false;
        if (asset.CurrentEmployeeId == null) throw InputError(nameof(Asset.CurrentEmployeeId), "The asset is not assigned.");

        var statusId = input.StatusId;
        if (statusId.HasValue)
        {
            var status = await dbContext.AssetStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == statusId, token);
            if (status == null) throw InputError(nameof(ReturnAssetInput.StatusId), "Unknown status.");
            if (status.Kind == StatusKind.Assigned) throw InputError(nameof(ReturnAssetInput.StatusId), "Pick a status that does not mean 'assigned'.");
        }
        statusId ??= await FindStatusId(StatusKind.Available, token)
            ?? throw InputError(nameof(ReturnAssetInput.StatusId), "No status of kind 'Available' is configured.");

        var now = DateTime.UtcNow;
        foreach (var open in (asset.Assignments ?? []).Where(a => a.ReturnedOn == null))
        {
            open.ReturnedOn = now;
            open.ReturnNotes = input.Notes;
        }
        asset.CurrentEmployeeId = null;
        asset.CurrentEmployee = null;
        asset.AssignedOn = null;
        asset.StatusId = statusId.Value;
        asset.Status = null;

        await Save(asset, token);
        return true;
    }

    private async Task Save(Asset asset, CancellationToken token)
    {
        workflow.IsTrustedWriter = true;
        try
        {
            await service.Modify(asset, token);
            await service.SaveChanges(token);
        }
        finally
        {
            workflow.IsTrustedWriter = false;
        }
    }

    private Task<int?> FindStatusId(StatusKind kind, CancellationToken token)
        => dbContext.AssetStatuses.AsNoTracking()
            .Where(x => x.Kind == kind)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(token);

    private static EntityInputException<Asset> InputError(string key, string message)
        => new("Asset workflow failed") { InputErrors = { [key] = message } };
}
