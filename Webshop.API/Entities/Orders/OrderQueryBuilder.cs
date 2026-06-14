using Regira.Entities.EFcore.Extensions;
using Regira.Entities.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Webshop.API.Entities.Orders;

public class OrderQueryBuilder : IFilteredQueryBuilder<Order, int, OrderSearchObject>
{
    public IQueryable<Order> Build(IQueryable<Order> query, OrderSearchObject? so)
    {
        if (so == null) return query;
        if (!string.IsNullOrWhiteSpace(so.Code))
            query = query.FilterCode(so.Code);
        if (so.CustomerId?.Any() == true)
            query = query.Where(x => so.CustomerId.Contains(x.CustomerId));
        if (so.ProductId?.Any() == true)
            query = query.Where(x => x.OrderLines!.Any(ol => so.ProductId.Contains(ol.ProductId)));
        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.OrderLines!.Any(ol => ol.Product!.Categories!.Any(pc => so.CategoryId.Contains(pc.CategoryId))));
        if (so.Status?.Any() == true)
            query = query.Where(x => so.Status.Contains(x.Status));
        if (so.MinCreatedDate.HasValue)
            query = query.Where(x => x.Created >= so.MinCreatedDate.Value);
        if (so.MaxCreatedDate.HasValue)
            query = query.Where(x => x.Created <= so.MaxCreatedDate.Value);
        return query;
    }
}
