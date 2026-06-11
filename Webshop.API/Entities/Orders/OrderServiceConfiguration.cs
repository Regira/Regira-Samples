using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Models;
using Webshop.API.Data;

namespace Webshop.API.Entities.Orders;

public static class OrderServiceConfiguration
{
    public static IEntityServiceCollection<WebshopDbContext> AddOrders(this IEntityServiceCollection<WebshopDbContext> services)
    {
        services.For<Order, OrderSearchObject, EntitySortBy, OrderIncludes>(e =>
        {
            e.AddFilter<OrderQueryBuilder>();
            e.SortBy((query, _) => query.OrderByDescending(x => x.Created));
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(OrderIncludes.Customer) == true)
                    query = query.Include(x => x.Customer!);
                if (includes?.HasFlag(OrderIncludes.OrderLines) == true)
                    query = query.Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                        .ThenInclude(ol => ol.Product!);
                return query;
            });
            e.Related(x => x.OrderLines, item => item.OrderLines?.SetSortOrder());
            e.Prepare((order, _) =>
            {
                if (string.IsNullOrWhiteSpace(order.Code))
                    order.Code = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
                if (order.OrderLines?.Any() == true)
                {
                    foreach (var line in order.OrderLines)
                        line.SubTotal = line.Quantity * line.UnitPrice;
                    order.Total = order.OrderLines.Sum(ol => ol.SubTotal);
                }
                return Task.CompletedTask;
            });
        });
        return services;
    }
}
