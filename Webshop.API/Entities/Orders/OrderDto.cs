using Webshop.API.Entities.Customers;
using Webshop.API.Entities.Products;

namespace Webshop.API.Entities.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public Guid? AggregateKey { get; set; }
    public string? Code { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerDto? Customer { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public string? ShippingAddress { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<OrderLineDto>? OrderLines { get; set; }
}

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
