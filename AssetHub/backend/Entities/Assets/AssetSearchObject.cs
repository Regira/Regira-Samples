using Regira.Entities.Models;

namespace AssetHub.Api.Entities.Assets;

public record AssetSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? StatusId { get; set; }
    public ICollection<int>? LocationId { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    /// <summary>Filter by whether the asset currently has an active (unreturned) assignment.</summary>
    public bool? IsAssigned { get; set; }
    public int? AssignedToEmployeeId { get; set; }
}
