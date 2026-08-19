using Regira.Entities.EFcore.Extensions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace Webshop.Api.Entities.Orders;

public class OrderQueryBuilder : IFilteredQueryBuilder<Order, int, OrderSearchObject>
{
    public IQueryable<Order> Build(IQueryable<Order> query, OrderSearchObject? so)
    {
        if (so == null) return query;

        if (!string.IsNullOrWhiteSpace(so.Code))
            query = query.FilterCode(so.Code);
        if (!string.IsNullOrWhiteSpace(so.CustomerEmail))
            query = query.Where(x => x.CustomerEmail == so.CustomerEmail);
        if (so.Status?.Any() == true)
            query = query.Where(x => so.Status.Contains(x.Status));
        if (so.ProductId?.Any() == true)
            query = query.Where(x => x.OrderLines!.Any(ol => so.ProductId.Contains(ol.ProductId)));
        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.OrderLines!.Any(ol => so.CategoryId.Contains(ol.Product!.CategoryId)));

        return query;
    }
}
