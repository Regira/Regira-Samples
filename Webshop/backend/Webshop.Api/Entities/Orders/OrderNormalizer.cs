using Microsoft.EntityFrameworkCore;
using Regira.Entities.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Orders;

// Folds customer + product text into the order so one ?q= hits all three.
public class OrderNormalizer(WebshopDbContext dbContext, INormalizer normalizer) : EntityNormalizerBase<Order>
{
    public override async Task HandleNormalize(Order item, CancellationToken token = default)
    {
        var productIds = item.OrderLines?.Select(ol => ol.ProductId).Distinct().ToList() ?? [];
        var productTitles = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Title)
            .ToListAsync(token);

        item.NormalizedContent = normalizer.Normalize(string.Join(' ',
            new[] { item.Code, item.CustomerName, item.CustomerEmail }
                .Concat(productTitles)
                .Where(s => !string.IsNullOrWhiteSpace(s))));
    }
}
