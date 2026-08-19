using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.AssetAssignments;

public class AssetAssignmentInputDto
{
    public int Id { get; set; }
    [Required]
    public int AssetId { get; set; }
    [Required]
    public int EmployeeId { get; set; }
    [Required]
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
}
