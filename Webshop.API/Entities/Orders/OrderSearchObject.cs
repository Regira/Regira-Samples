using Regira.Entities.Models;

namespace Webshop.API.Entities.Orders;

public record OrderSearchObject : SearchObject
{
    public string? Code { get; set; }
    public ICollection<Guid>? CustomerId { get; set; }
    public ICollection<int>? ProductId { get; set; }
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<OrderStatus>? Status { get; set; }
}
