using Webshop.API.Entities.Categories;

namespace Webshop.API.Entities.Products;

public class ProductCategoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
}
