using System.ComponentModel.DataAnnotations;

namespace Webshop.API.Entities.Orders;

public class OrderInputDto
{
    public int Id { get; set; }
    [MaxLength(32)] public string? Code { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    [MaxLength(1024)] public string? Notes { get; set; }
    public ICollection<OrderLineInputDto>? OrderLines { get; set; }
}
