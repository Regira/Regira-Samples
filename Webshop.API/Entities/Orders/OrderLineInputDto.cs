namespace Webshop.API.Entities.Orders;

public class OrderLineInputDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}
