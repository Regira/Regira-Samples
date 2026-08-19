using Webshop.Api.Entities.Categories;

namespace Webshop.Api.Entities.Products;

public class ProductCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
}

public class ProductDto : ProductCoreDto
{
    public string? Description { get; set; }
    public string? Code { get; set; }
    public string? Brand { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
