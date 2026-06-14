using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using Webshop.API.Data;

namespace Webshop.API.Entities.Orders;

public static class OrderServiceConfiguration
{
    public static EntityServiceCollection<WebshopDbContext> AddOrders(
        this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Order, OrderSearchObject, EntitySortBy, OrderIncludes>(e =>
        {
            e.UseMapping<OrderDto, OrderInputDto>();
            e.AddMapping<OrderLineDto, OrderLineDto>();
            e.AddMapping<OrderLineInputDto, OrderLine>();

            e.AddFilter<OrderQueryBuilder>();

            e.SortBy((query, sortBy) => query.OrderByDescending(x => x.Created));

            e.Includes((query, includes) => query
                .Include(x => x.Customer!)
                .Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                    .ThenInclude(ol => ol.Product!));

            e.Related(x => x.OrderLines);

            e.Prepare((order, original) =>
            {
                if (string.IsNullOrWhiteSpace(order.Code))
                    order.Code = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
                if (order.OrderLines?.Any() == true)
                {
                    var i = 0;
                    foreach (var line in order.OrderLines)
                    {
                        line.SortOrder = ++i;
                        line.SubTotal = line.Quantity * line.UnitPrice;
                    }
                    order.Total = order.OrderLines.Sum(ol => ol.SubTotal);
                }
                return Task.CompletedTask;
            });
        });
}
