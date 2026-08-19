using AssetHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;

namespace AssetHub.Api.Entities.AssetAssignments;

// Enforces "at most one active assignment per asset" -- a business invariant no FK/unique index expresses.
public class AssetAssignmentPrepper(AppDbContext dbContext) : EntityPrepperBase<AssetAssignment>
{
    public override async Task Prepare(AssetAssignment modified, AssetAssignment? original, CancellationToken token = default)
    {
        if (modified.ReturnedDate != null && modified.ReturnedDate < modified.AssignedDate)
        {
            throw new EntityInputException<AssetAssignment>("Saving assignment failed")
            {
                InputErrors = { ["ReturnedDate"] = "Return date cannot be before the assigned date." }
            };
        }

        if (modified.ReturnedDate == null)
        {
            var hasOtherActive = await dbContext.AssetAssignments
                .AnyAsync(a => a.AssetId == modified.AssetId && a.ReturnedDate == null && a.Id != modified.Id, token);
            if (hasOtherActive)
            {
                throw new EntityInputException<AssetAssignment>("Saving assignment failed")
                {
                    InputErrors = { ["AssetId"] = "This asset already has an active assignment -- return it first." }
                };
            }
        }
    }
}
