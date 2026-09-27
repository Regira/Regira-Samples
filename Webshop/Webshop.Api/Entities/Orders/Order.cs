using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Webshop.Api.Entities.Orders;

public class Order : IEntityWithSerial, IHasTimestamps, IHasCode, IHasNormalizedContent
{
    public int Id { get; set; }
    /// <summary>Public order number: server-owned, minted on create (e.ServerOwned in OrderServiceConfiguration)</summary>
    [MaxLength(16)] public string? Code { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // customer (guest checkout: a snapshot on the order)
    [Required, MaxLength(128)] public string CustomerName { get; set; } = null!;
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(32)] public string? Phone { get; set; }

    // shipping address
    [Required, MaxLength(256)] public string Street { get; set; } = null!;
    [Required, MaxLength(16)] public string PostalCode { get; set; } = null!;
    [Required, MaxLength(128)] public string City { get; set; } = null!;
    [Required, MaxLength(64)] public string Country { get; set; } = null!;

    public ShippingMethod ShippingMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }

    // computed by the Order prepper on every save (never trusted from the client)
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }

    public ICollection<OrderLine>? OrderLines { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(CustomerName), nameof(Email), nameof(City)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
