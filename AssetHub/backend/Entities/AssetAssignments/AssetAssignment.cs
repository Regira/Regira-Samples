using System.ComponentModel.DataAnnotations;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.Employees;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.AssetAssignments;

// Budget: complex 2/2. Top-level (not owned) -- needs to be independently searchable/sortable
// (assignment history per employee, per asset) and references two other aggregates.
public class AssetAssignment : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AssignedDate { get; set; }
    /// <summary>Null while the assignment is active (asset not yet returned).</summary>
    public DateTime? ReturnedDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
