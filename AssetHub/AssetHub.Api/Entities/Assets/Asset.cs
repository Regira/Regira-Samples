using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace AssetHub.Api.Entities.Assets;

public class Asset : IEntityWithSerial, IHasCode, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent,
    IHasAttachments, IHasAttachments<AssetAttachment>
{
    public int Id { get; set; }
    /// <summary>Asset tag, e.g. AST-00042. Minted on create when left empty.</summary>
    [MaxLength(16)] public string? Code { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(64)] public string? SerialNumber { get; set; }
    [MaxLength(64)] public string? Manufacturer { get; set; }
    [MaxLength(64)] public string? Model { get; set; }
    [MaxLength(2048)] public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int StatusId { get; set; }
    public AssetStatus? Status { get; set; }
    public int? LocationId { get; set; }
    public Location? Location { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    [MaxLength(32)] public string? OrderNumber { get; set; }

    // Written only by the assign / return workflow (AssetWorkflowGuard restores them on ordinary writes)
    public int? CurrentEmployeeId { get; set; }
    public Employee? CurrentEmployee { get; set; }
    public DateTime? AssignedOn { get; set; }

    public ICollection<AssetAssignment>? Assignments { get; set; }
    public ICollection<Warranty>? Warranties { get; set; }
    public ICollection<MaintenanceRecord>? MaintenanceRecords { get; set; }

    [NotMapped] public bool? HasAttachment { get; set; }
    public ICollection<AssetAttachment>? Attachments { get; set; }
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<AssetAttachment>().ToArray();
    }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(Title), nameof(SerialNumber), nameof(Manufacturer), nameof(Model)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>One row per hand-over: the complete assignment history of an asset (owned by Asset)</summary>
public class AssetAssignment : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime AssignedOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
    [MaxLength(512)] public string? Notes { get; set; }
    [MaxLength(512)] public string? ReturnNotes { get; set; }
}

public enum WarrantyType
{
    Manufacturer = 0,
    Extended = 1,
    ServiceContract = 2
}

/// <summary>Owned by Asset (edited through the asset form)</summary>
public class Warranty : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public WarrantyType Type { get; set; }
    [Required, MaxLength(96)] public string? Provider { get; set; }
    [MaxLength(64)] public string? Reference { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? Cost { get; set; }
    [MaxLength(512)] public string? Coverage { get; set; }
}

public enum MaintenanceType
{
    Preventive = 0,
    Repair = 1,
    Inspection = 2,
    Upgrade = 3,
    Cleaning = 4
}

/// <summary>Owned by Asset (edited through the asset form)</summary>
public class MaintenanceRecord : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public MaintenanceType Type { get; set; }
    public DateOnly Date { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(96)] public string? PerformedBy { get; set; }
    public decimal? Cost { get; set; }
    public DateOnly? NextDueDate { get; set; }
}
