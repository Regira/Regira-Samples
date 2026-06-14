using Webshop.API.Entities.Customers;

namespace Webshop.API.Entities.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerDto? Customer { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<OrderLineDto>? OrderLines { get; set; }
}
