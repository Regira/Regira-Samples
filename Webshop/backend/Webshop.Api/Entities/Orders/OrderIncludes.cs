namespace Webshop.Api.Entities.Orders;

[Flags]
public enum OrderIncludes
{
    Default = 0,
    OrderLines = 1 << 0,
    All = OrderLines
}
