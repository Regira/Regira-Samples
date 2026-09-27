using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using Webshop.Api.Entities.Brands;
using Webshop.Api.Entities.Categories;

namespace Webshop.Api.Entities.Products;

public class Product : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [Required, MaxLength(32)] public string Sku { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    public decimal Price { get; set; }
    /// <summary>Original price; when higher than <see cref="Price"/> the product is on sale</summary>
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
    /// <summary>Average review score, 0-5</summary>
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    [MaxLength(512)] public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    /// <summary>Published in the storefront</summary>
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Sku), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
