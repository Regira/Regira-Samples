using Regira.Entities.QueryBuilders.Abstractions;

namespace Webshop.Api.Entities.Orders;

public class OrderQueryBuilder : FilteredQueryBuilderBase<Order, int, OrderSearchObject>
{
    public override IQueryable<Order> Build(IQueryable<Order> query, OrderSearchObject? so)
    {
        if (so == null) return query;
        if (!string.IsNullOrWhiteSpace(so.Code)) query = query.Where(x => x.Code == so.Code.Trim().ToUpper());
        if (!string.IsNullOrWhiteSpace(so.Email)) query = query.Where(x => x.Email == so.Email.Trim().ToLower());
        if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
        if (so.ProductId?.Any() == true) query = query.Where(x => x.OrderLines!.Any(l => so.ProductId.Contains(l.ProductId)));
        if (so.MinTotal.HasValue) query = query.Where(x => x.Total >= so.MinTotal.Value);
        if (so.MaxTotal.HasValue) query = query.Where(x => x.Total <= so.MaxTotal.Value);
        return query;
    }
}
