using Webshop.Api.Entities.Products;

namespace Webshop.Api.Entities.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public OrderStatus Status { get; set; }

    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string? CustomerPhone { get; set; }

    public string ShippingAddress { get; set; } = null!;
    public string ShippingCity { get; set; } = null!;
    public string ShippingPostalCode { get; set; } = null!;
    public string ShippingCountry { get; set; } = null!;

    public decimal Total { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<OrderLineDto>? OrderLines { get; set; }
}

public class OrderLineDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public ProductCoreDto? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int SortOrder { get; set; }
}
