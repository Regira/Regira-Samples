using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using Webshop.Api.Entities.Categories;

namespace Webshop.Api.Entities.Products;

public class Product : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasSlug, IHasCode, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;
    [MaxLength(64)]
    public string? Slug { get; set; }
    [MaxLength(2048)]
    public string? Description { get; set; }
    [MaxLength(32)]
    public string? Code { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [MaxLength(64)]
    public string? Brand { get; set; }
    [MaxLength(512)]
    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description), nameof(Brand)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
