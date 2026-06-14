using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Webshop.API.Entities.Customers;

namespace Webshop.API.Entities.Orders;

public class Order : IEntityWithSerial, IHasTimestamps, IHasCode
{
    public int Id { get; set; }
    [MaxLength(32)] public string? Code { get; set; }
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<OrderLine>? OrderLines { get; set; }
}
