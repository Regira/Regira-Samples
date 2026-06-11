namespace Webshop.API.Entities.Orders;

[Flags]
public enum OrderIncludes
{
    Default = 0,
    Customer = 1 << 0,
    OrderLines = 1 << 1,
    All = Customer | OrderLines
}
