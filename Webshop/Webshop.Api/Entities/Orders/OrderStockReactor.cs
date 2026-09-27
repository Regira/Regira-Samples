using Microsoft.EntityFrameworkCore;
using Regira.Entities.Reactors.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Orders;

/// <summary>
/// After the commit: a new order takes its quantities out of stock, a cancelled order puts them back
/// (and an un-cancelled order takes them out again).
/// </summary>
public class OrderStockReactor(WebshopDbContext dbContext) : EntityReactorBase<Order>
{
    public override bool CanReact(IEntityChange<Order> change)
        => (change.Kind == EntityChangeKind.Added && change.Entity.Status != OrderStatus.Cancelled)
           || (change.Kind == EntityChangeKind.Modified && change.HasChanged(x => x.Status)
               && (change.Entity.Status == OrderStatus.Cancelled || change.Original?.Status == OrderStatus.Cancelled));

    public override async Task React(IEntityChange<Order> change, CancellationToken token = default)
    {
        // +1 = restock (cancelled), -1 = take out of stock (placed or un-cancelled)
        var direction = change.Kind == EntityChangeKind.Modified && change.Entity.Status == OrderStatus.Cancelled ? 1 : -1;
        var quantities = await dbContext.OrderLines.AsNoTracking()
            .Where(l => l.OrderId == change.Entity.Id)
            .GroupBy(l => l.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(l => l.Quantity) })
            .ToListAsync(token);
        foreach (var q in quantities)
        {
            var delta = direction * q.Quantity;
            await dbContext.Products
                .Where(p => p.Id == q.ProductId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + delta < 0 ? 0 : p.Stock + delta), token);
        }
    }
}
