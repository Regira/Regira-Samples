using Regira.Entities.Models;

namespace Fleet.API.Entities.Suppliers;

public record SupplierSearchObject : SearchObject
{
    public string? Title { get; set; }
    public string? City { get; set; }
    /// <summary>Only return suppliers able to perform any of these intervention types.</summary>
    public ICollection<int>? InterventionTypeId { get; set; }
}
