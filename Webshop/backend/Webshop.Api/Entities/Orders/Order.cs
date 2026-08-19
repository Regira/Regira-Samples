using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace Webshop.Api.Entities.Orders;

// Guest checkout: no separate Customer entity - keeps the free-tier budget lean and matches
// a typical webshop checkout (name/e-mail/address captured per order).
public class Order : IEntityWithSerial, IHasTimestamps, IHasCode, IHasNormalizedContent
{
    public int Id { get; set; }
    [MaxLength(16)]
    public string? Code { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required, MaxLength(128)]
    public string CustomerName { get; set; } = null!;
    [Required, MaxLength(256), EmailAddress]
    public string CustomerEmail { get; set; } = null!;
    [MaxLength(32)]
    public string? CustomerPhone { get; set; }

    [Required, MaxLength(256)]
    public string ShippingAddress { get; set; } = null!;
    [Required, MaxLength(64)]
    public string ShippingCity { get; set; } = null!;
    [Required, MaxLength(16)]
    public string ShippingPostalCode { get; set; } = null!;
    [Required, MaxLength(64)]
    public string ShippingCountry { get; set; } = null!;

    public decimal Total { get; set; }

    // Populated by OrderNormalizer (needs a DB lookup of order-line product titles, so an attribute-based
    // [Normalized] source can't reach it).
    [MaxLength(1024)]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<OrderLine>? OrderLines { get; set; }
}
