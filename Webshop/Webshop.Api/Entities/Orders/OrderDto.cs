using Webshop.Api.Entities.Products;

namespace Webshop.Api.Entities.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public OrderStatus Status { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Street { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public ShippingMethod ShippingMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public ICollection<OrderLineDto>? OrderLines { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class OrderLineDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public ProductDto? Product { get; set; }
    public string? ProductTitle { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }
}

public class OrderInputDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Street { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public ShippingMethod ShippingMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    // null = not sent (lines untouched) | [] = delete all (rejected) | populated = the new set
    public ICollection<OrderLineInputDto>? OrderLines { get; set; }
}

/// <summary>No price fields: unit prices are resolved server-side (price-tampering guard)</summary>
public class OrderLineInputDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
