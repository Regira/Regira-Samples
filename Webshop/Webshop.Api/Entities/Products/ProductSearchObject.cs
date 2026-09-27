using Regira.Entities.Models;

namespace Webshop.Api.Entities.Products;

public record ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public bool? OnSale { get; set; }
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; }
}
