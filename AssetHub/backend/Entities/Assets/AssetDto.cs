using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;

namespace AssetHub.Api.Entities.Assets;

public class AssetDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }

    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }

    public int StatusId { get; set; }
    public AssetStatusDto? Status { get; set; }

    public int? LocationId { get; set; }
    public LocationDto? Location { get; set; }

    public int? SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? Notes { get; set; }

    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<AssetAttachmentDto>? Attachments { get; set; }
    public ICollection<AssetWarrantyDto>? Warranties { get; set; }
    public ICollection<AssetMaintenanceRecordDto>? MaintenanceRecords { get; set; }
    public ICollection<AssetAssignmentSummaryDto>? Assignments { get; set; }

    public int? CurrentEmployeeId { get; set; }
    public string? CurrentEmployeeName { get; set; }
    public DateTime? CurrentAssignedDate { get; set; }
}
