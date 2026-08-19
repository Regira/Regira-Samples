using Regira.Entities.Models.Abstractions;
using Webshop.Api.Entities.Products;

namespace Webshop.Api.Entities.Orders;

// Owned child of Order via e.Related() - no own .For<>(), no controller, no budget slot.
public class OrderLine : IEntityWithSerial, IHasTimestamps, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    // Server-owned - resolved from Product.Price in Order's Prepare hook (price-tampering guard).
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int SortOrder { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
