using Webshop.Api.Entities.Brands;
using Webshop.Api.Entities.Categories;

namespace Webshop.Api.Entities.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
    public int? BrandId { get; set; }
    public BrandCoreDto? Brand { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class ProductInputDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
}
