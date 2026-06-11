using Regira.Entities.Models.Abstractions;
using Webshop.API.Entities.Products;

namespace Webshop.API.Entities.Orders;

public class OrderLine : IEntityWithSerial, IHasTimestamps, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
