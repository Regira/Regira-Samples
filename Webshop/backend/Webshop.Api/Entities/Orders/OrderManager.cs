using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;

namespace Webshop.Api.Entities.Orders;

public interface IOrderService : IEntityService<Order, OrderSearchObject, EntitySortBy, OrderIncludes>;

// The controller write path calls Save(), which the base routes to Add()/Modify() based on
// IEntity.IsNew() - so validation + code generation belong there, not in an overridden Save().
public class OrderManager(IEntityRepository<Order, OrderSearchObject, EntitySortBy, OrderIncludes> service)
    : EntityWrappingServiceBase<Order, OrderSearchObject, EntitySortBy, OrderIncludes>(service), IOrderService
{
    public override Task Add(Order item, CancellationToken token = default)
    {
        RequireLines(item.OrderLines?.Any() == true);
        if (string.IsNullOrWhiteSpace(item.Code))
            item.Code = $"ORD-{Guid.NewGuid():N}"[..12].ToUpperInvariant(); // fits Code's [MaxLength(16)]
        return base.Add(item, token);
    }

    public override Task<Order?> Modify(Order item, CancellationToken token = default)
    {
        // null = not sent (a status-only PATCH) - leave stored lines alone; [] = explicit delete-all,
        // which would strand the order without lines, so it is rejected the same as "no lines".
        RequireLines(item.OrderLines is not { Count: 0 });
        return base.Modify(item, token);
    }

    private static void RequireLines(bool hasLines)
    {
        if (!hasLines)
            throw new EntityInputException<Order>("Saving order failed")
            {
                InputErrors = { ["OrderLines"] = "Order must contain at least one order line." }
            };
    }
}
