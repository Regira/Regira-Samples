using System.ComponentModel.DataAnnotations;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Models;

namespace AssetHub.Api.Entities.Assets;

public record AssetSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? StatusId { get; set; }
    public ICollection<StatusKind>? StatusKind { get; set; }
    public ICollection<int>? LocationId { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    /// <summary>Current holder</summary>
    public ICollection<int>? EmployeeId { get; set; }
    public bool? IsAssigned { get; set; }
    public bool? HasAttachment { get; set; }
    /// <summary>Has an active warranty ending on or before this date</summary>
    public DateOnly? WarrantyExpiresBefore { get; set; }
    /// <summary>true = at least one warranty covers today</summary>
    public bool? UnderWarranty { get; set; }
    /// <summary>A maintenance record has NextDueDate on or before this date</summary>
    public DateOnly? MaintenanceDueBefore { get; set; }
    public DateOnly? MinPurchaseDate { get; set; }
    public DateOnly? MaxPurchaseDate { get; set; }
}

public enum AssetSortBy
{
    Default = 0,
    Code,
    CodeDesc,
    Title,
    TitleDesc,
    PurchaseDate,
    PurchaseDateDesc,
    PurchasePrice,
    PurchasePriceDesc,
    Created,
    CreatedDesc,
    LastModified,
    LastModifiedDesc
}

[Flags]
public enum AssetIncludes
{
    Default = 0,
    Assignments = 1 << 0,
    Warranties = 1 << 1,
    MaintenanceRecords = 1 << 2,
    Attachments = 1 << 3,
    All = Assignments | Warranties | MaintenanceRecords | Attachments
}

// ---------- read DTOs ----------

/// <summary>Flat asset shape used inside other DTOs (no collections)</summary>
public class AssetCoreDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? SerialNumber { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public int StatusId { get; set; }
    public AssetStatusDto? Status { get; set; }
}

public class EmployeeRefDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
}

public class AssetDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public int StatusId { get; set; }
    public AssetStatusDto? Status { get; set; }
    public int? LocationId { get; set; }
    public LocationDto? Location { get; set; }
    public int? SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? OrderNumber { get; set; }
    public int? CurrentEmployeeId { get; set; }
    public EmployeeRefDto? CurrentEmployee { get; set; }
    public DateTime? AssignedOn { get; set; }
    public ICollection<AssetAssignmentDto>? Assignments { get; set; }
    public ICollection<WarrantyDto>? Warranties { get; set; }
    public ICollection<MaintenanceRecordDto>? MaintenanceRecords { get; set; }
    public ICollection<EntityAttachmentDto>? Attachments { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class AssetAssignmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeRefDto? Employee { get; set; }
    public DateTime AssignedOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
    public string? Notes { get; set; }
    public string? ReturnNotes { get; set; }
}

public class WarrantyDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public WarrantyType Type { get; set; }
    public string? Provider { get; set; }
    public string? Reference { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Coverage { get; set; }
}

public class MaintenanceRecordDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public MaintenanceType Type { get; set; }
    public DateOnly Date { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PerformedBy { get; set; }
    public decimal? Cost { get; set; }
    public DateOnly? NextDueDate { get; set; }
}

// ---------- input DTOs ----------

public class AssetInputDto
{
    public int Id { get; set; }
    [MaxLength(16)] public string? Code { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(64)] public string? SerialNumber { get; set; }
    [MaxLength(64)] public string? Manufacturer { get; set; }
    [MaxLength(64)] public string? Model { get; set; }
    [MaxLength(2048)] public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    [Range(1, int.MaxValue)] public int StatusId { get; set; }
    public int? LocationId { get; set; }
    public int? SupplierId { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    [Range(0, 10_000_000)] public decimal? PurchasePrice { get; set; }
    [MaxLength(32)] public string? OrderNumber { get; set; }
    // CurrentEmployeeId / AssignedOn / Assignments deliberately absent: the assign / return workflow writes them.
    // Owned collections: nullable + uninitialized (null = untouched, [] = delete all)
    public ICollection<WarrantyInputDto>? Warranties { get; set; }
    public ICollection<MaintenanceRecordInputDto>? MaintenanceRecords { get; set; }
    public ICollection<EntityAttachmentInputDto>? Attachments { get; set; }
}

public class WarrantyInputDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public WarrantyType Type { get; set; }
    [Required, MaxLength(96)] public string? Provider { get; set; }
    [MaxLength(64)] public string? Reference { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    [Range(0, 1_000_000)] public decimal? Cost { get; set; }
    [MaxLength(512)] public string? Coverage { get; set; }
}

public class MaintenanceRecordInputDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public MaintenanceType Type { get; set; }
    public DateOnly Date { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(96)] public string? PerformedBy { get; set; }
    [Range(0, 1_000_000)] public decimal? Cost { get; set; }
    public DateOnly? NextDueDate { get; set; }
}

// ---------- workflow inputs ----------

public class AssignAssetInput
{
    [Range(1, int.MaxValue)] public int EmployeeId { get; set; }
    [MaxLength(512)] public string? Notes { get; set; }
}

public class ReturnAssetInput
{
    [MaxLength(512)] public string? Notes { get; set; }
    /// <summary>Optional status after return; defaults to the first "Available" status</summary>
    public int? StatusId { get; set; }
}
