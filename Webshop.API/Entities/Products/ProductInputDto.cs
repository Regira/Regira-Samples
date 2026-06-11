using System.ComponentModel.DataAnnotations;

namespace Webshop.API.Entities.Products;

public class ProductInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ICollection<ProductCategoryInputDto>? Categories { get; set; }
}

public class ProductCategoryInputDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
}
