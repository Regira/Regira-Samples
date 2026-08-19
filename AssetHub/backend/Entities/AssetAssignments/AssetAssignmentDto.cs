using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.Employees;

namespace AssetHub.Api.Entities.AssetAssignments;

public class AssetAssignmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public AssetDto? Asset { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
