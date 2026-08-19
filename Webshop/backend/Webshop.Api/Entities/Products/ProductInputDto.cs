using System.ComponentModel.DataAnnotations;

namespace Webshop.Api.Entities.Products;

public class ProductInputDto
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
    [Required]
    public int CategoryId { get; set; }
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
}
