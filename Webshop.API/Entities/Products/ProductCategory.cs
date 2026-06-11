using Regira.Entities.Models.Abstractions;
using Webshop.API.Entities.Categories;

namespace Webshop.API.Entities.Products;

public class ProductCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
