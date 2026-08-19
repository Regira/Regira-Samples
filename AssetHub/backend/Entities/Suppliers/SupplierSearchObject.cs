using Regira.Entities.Models;

namespace AssetHub.Api.Entities.Suppliers;

public record SupplierSearchObject : SearchObject
{
    public string? Title { get; set; }
}
