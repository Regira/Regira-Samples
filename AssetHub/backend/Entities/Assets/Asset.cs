using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetHub.Api.Entities.AssetAssignments;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace AssetHub.Api.Entities.Assets;

// Budget: complex 1/2. Aggregate parent (owns attachments/warranties/maintenance records) -> IArchivable is
// the intended shape here; the PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning EF logs
// for its owned dependents is expected noise for an aggregate parent (suppressed on the DbContext options).
public class Asset : IEntityWithSerial, IHasTitle, IHasDescription, IHasCode, IHasTimestamps, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }
    /// <summary>Human-friendly asset tag, e.g. "AST-A1B2C3D4". Server-generated when left blank.</summary>
    [MaxLength(20)]
    public string? Code { get; set; }
    [Required, MaxLength(150)]
    public string Title { get; set; } = null!;
    [MaxLength(1000)]
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int StatusId { get; set; }
    public AssetStatus? Status { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description), nameof(SerialNumber), nameof(Code)])]
    public string? NormalizedContent { get; set; }

    public ICollection<AssetAttachment>? Attachments { get; set; }
    public ICollection<AssetWarranty>? Warranties { get; set; }
    public ICollection<AssetMaintenanceRecord>? MaintenanceRecords { get; set; }

    // Back-reference only -- AssetAssignment is its own top-level (complex) registration, not owned via Related().
    public ICollection<AssetAssignment>? Assignments { get; set; }

    // Filled by AssetProcessor from the currently active (unreturned) assignment, if any.
    [NotMapped] public int? CurrentEmployeeId { get; set; }
    [NotMapped] public string? CurrentEmployeeName { get; set; }
    [NotMapped] public DateTime? CurrentAssignedDate { get; set; }
}
