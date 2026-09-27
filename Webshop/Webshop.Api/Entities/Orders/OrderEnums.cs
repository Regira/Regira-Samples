namespace Webshop.Api.Entities.Orders;

public enum OrderStatus { Pending = 0, Paid, Processing, Shipped, Delivered, Cancelled }
public enum PaymentMethod { Card = 0, PayPal, BankTransfer, CashOnDelivery }
public enum ShippingMethod { Standard = 0, Express, Pickup }

[Flags]
public enum OrderIncludes { Default = 0, Lines = 1 << 0, All = Lines }

public enum OrderSortBy { Default = 0, Oldest, TotalDesc, TotalAsc, Code, Customer }
