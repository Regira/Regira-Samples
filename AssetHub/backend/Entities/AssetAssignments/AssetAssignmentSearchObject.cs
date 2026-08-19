using Regira.Entities.Models;

namespace AssetHub.Api.Entities.AssetAssignments;

public record AssetAssignmentSearchObject : SearchObject
{
    public ICollection<int>? AssetId { get; set; }
    public ICollection<int>? EmployeeId { get; set; }
    /// <summary>true = only active (ReturnedDate == null); false = only returned.</summary>
    public bool? IsActive { get; set; }
    public DateTime? MinAssignedDate { get; set; }
    public DateTime? MaxAssignedDate { get; set; }
}
