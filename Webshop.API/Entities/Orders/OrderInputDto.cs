using System.ComponentModel.DataAnnotations;

namespace Webshop.API.Entities.Orders;

public class OrderInputDto
{
    public int Id { get; set; }
    public Guid? AggregateKey { get; set; }
    [MaxLength(16)] public string? Code { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? ShippingAddress { get; set; }
    public ICollection<OrderLineInputDto>? OrderLines { get; set; }
}

public class OrderLineInputDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
