using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Extensions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Orders;

public static class OrderServiceConfiguration
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string NewOrderCode()
        => "WS-" + new string(Enumerable.Range(0, 8).Select(_ => CodeAlphabet[Random.Shared.Next(CodeAlphabet.Length)]).ToArray());

    // complex registration (1 complex slot): typed sorting + named includes; OrderLine is an owned child (no slot)
    public static EntityServiceCollection<WebshopDbContext> AddOrders(this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
        {
            e.AddFilter<OrderQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                OrderSortBy.Oldest => query.OrderOrThenBy(x => x.Created),
                OrderSortBy.TotalDesc => query.OrderOrThenByDescending(x => x.Total),
                OrderSortBy.TotalAsc => query.OrderOrThenBy(x => x.Total),
                OrderSortBy.Code => query.OrderOrThenBy(x => x.Code),
                OrderSortBy.Customer => query.OrderOrThenBy(x => x.CustomerName),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });
            // lines are a collection: flag-gated (Details loads every flag; lists opt in with ?includes=Lines)
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(OrderIncludes.Lines) == true)
                    query = query.Include(x => x.OrderLines!.OrderBy(l => l.SortOrder))
                        .ThenInclude(l => l.Product!).ThenInclude(p => p.Category);
                return query;
            });
            e.ServerOwned(x => x.Code, _ => NewOrderCode());
            e.Related(x => x.OrderLines, order => order.OrderLines?.SetSortOrder());
            e.AddPrepper<OrderPrepper>();
            e.AddReactor<OrderStockReactor>();
        });
}
