using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Models;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Orders;

public static class OrderServiceConfiguration
{
    // Budget: complex registration (2/2 complex) - OrderLines need typed includes to opt in per request.
    public static EntityServiceCollection<WebshopDbContext> AddOrders(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Order, OrderSearchObject, EntitySortBy, OrderIncludes>(e =>
        {
            e.AddFilter<OrderQueryBuilder>();
            e.SortBy((query, _) => query.OrderByDescending(x => x.Created));
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(OrderIncludes.OrderLines) == true)
                    query = query.Include(x => x.OrderLines!.OrderBy(l => l.SortOrder)).ThenInclude(ol => ol.Product);
                return query;
            });
            e.Related(x => x.OrderLines, item => item.OrderLines?.SetSortOrder());
            e.Prepare(async (order, dbContext) =>
            {
                // Three-way on the incoming collection (the Related() contract):
                //   null = not sent, stored lines untouched | [] = delete-all | populated = the new set.
                if (order.OrderLines == null)
                {
                    order.Total = order.Id > 0
                        ? await dbContext.OrderLines.AsNoTracking()
                            .Where(l => l.OrderId == order.Id)
                            .SumAsync(l => l.SubTotal)
                        : 0m;
                    return;
                }

                // UnitPrice/SubTotal are server-owned: resolve from the Product, never trust client input.
                var productIds = order.OrderLines.Select(l => l.ProductId).ToList();
                var prices = await dbContext.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Price);
                foreach (var line in order.OrderLines)
                {
                    line.UnitPrice = prices.GetValueOrDefault(line.ProductId);
                    line.SubTotal = line.Quantity * line.UnitPrice;
                }
                order.Total = order.OrderLines.Sum(line => line.SubTotal);
            });
            e.AddNormalizer<OrderNormalizer>();
            e.AddTransient<IOrderService, OrderManager>();
            e.UseEntityService<OrderManager>();
        });
}
