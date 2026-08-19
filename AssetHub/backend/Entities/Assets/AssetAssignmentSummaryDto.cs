using AssetHub.Api.Entities.Employees;

namespace AssetHub.Api.Entities.Assets;

// Read-only projection of AssetAssignment for embedding inside AssetDto.Assignments.
// Deliberately has no Asset/AssetId back-reference: the owning asset IS this asset, so including it
// would (a) be redundant and (b) let Mapster's object-graph mapping recurse through
// Asset -> Assignments -> Asset -> Assignments -> ... via EF's relationship fixup on the tracked
// parent -- a real stack overflow at mapping time, not just a JSON reference cycle.
public class AssetAssignmentSummaryDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string? Notes { get; set; }
}
