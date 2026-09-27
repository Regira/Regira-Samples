using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Webshop.Api.Entities.Products;

namespace Webshop.Api.Entities.Orders;

/// <summary>Owned child of <see cref="Order"/> (synced via e.Related, no own registration)</summary>
public class OrderLine : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    // snapshots taken when the line is first saved (server-owned)
    [MaxLength(128)] public string? ProductTitle { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}
