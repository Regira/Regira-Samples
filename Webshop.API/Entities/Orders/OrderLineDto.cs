using Webshop.API.Entities.Products;

namespace Webshop.API.Entities.Orders;

public class OrderLineDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public ProductDto? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int SortOrder { get; set; }
}
