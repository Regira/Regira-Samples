using Regira.Entities.Models;

namespace Webshop.Api.Entities.Products;

public record ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<string>? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? OnSale { get; set; }
    // Exact match - lets the SPA resolve a product detail page straight from its URL slug.
    public string? Slug { get; set; }
}
