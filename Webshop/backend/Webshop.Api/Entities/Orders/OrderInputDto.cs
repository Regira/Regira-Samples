using System.ComponentModel.DataAnnotations;

namespace Webshop.Api.Entities.Orders;

public class OrderInputDto
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

    // Only null (untouched) or the full set is meaningful here - OrderManager rejects [] (delete-all).
    public ICollection<OrderLineInputDto>? OrderLines { get; set; }
}

// No UnitPrice/SubTotal - both are server-owned, resolved from Product.Price in Order's Prepare hook
// (price-tampering guard).
public class OrderLineInputDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    [Required]
    public int ProductId { get; set; }
    [Range(1, 999)]
    public int Quantity { get; set; }
}
