namespace Webshop.Api.Entities.Orders;

public static class OrderPricing
{
    public const decimal FreeShippingThreshold = 50m;
    public const decimal StandardShipping = 4.95m;
    public const decimal ExpressShipping = 9.95m;

    public static decimal ShippingCost(ShippingMethod method, decimal subtotal) => method switch
    {
        ShippingMethod.Express => ExpressShipping,
        ShippingMethod.Pickup => 0m,
        _ => subtotal >= FreeShippingThreshold ? 0m : StandardShipping
    };
}
