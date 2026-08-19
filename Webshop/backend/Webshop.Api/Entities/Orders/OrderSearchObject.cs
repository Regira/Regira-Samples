using Regira.Entities.Models;

namespace Webshop.Api.Entities.Orders;

public record OrderSearchObject : SearchObject
{
    public string? Code { get; set; }
    public string? CustomerEmail { get; set; }
    public ICollection<OrderStatus>? Status { get; set; }
    public ICollection<int>? ProductId { get; set; }
    public ICollection<int>? CategoryId { get; set; }
}
