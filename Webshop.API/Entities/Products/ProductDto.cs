using Webshop.API.Entities.Categories;

namespace Webshop.API.Entities.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ProductCategoryDto>? Categories { get; set; }
}

public class ProductCategoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}
