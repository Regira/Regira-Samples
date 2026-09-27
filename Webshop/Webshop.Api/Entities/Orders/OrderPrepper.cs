using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;
using Webshop.Api.Data;

namespace Webshop.Api.Entities.Orders;

/// <summary>
/// Validates the lines and computes every server-owned amount of an order.
/// OrderLines is three-way: null = not sent (recompute from the stored lines), [] = rejected, populated = the new set.
/// </summary>
public class OrderPrepper(WebshopDbContext dbContext) : EntityPrepperBase<Order>
{
    public const int MaxQuantityPerLine = 99;

    public override async Task Prepare(Order modified, Order? original, CancellationToken token = default)
    {
        modified.Email = modified.Email?.Trim().ToLowerInvariant()!;

        if (modified.OrderLines == null)
        {
            var stored = modified.Id > 0
                ? await dbContext.OrderLines.AsNoTracking().Where(l => l.OrderId == modified.Id)
                    .Select(l => new { l.Quantity, l.LineTotal }).ToListAsync(token)
                : [];
            ApplyTotals(modified, stored.Sum(l => l.LineTotal), stored.Sum(l => l.Quantity));
            return;
        }

        var errors = new Dictionary<string, string>();
        if (modified.OrderLines.Count == 0)
            errors["OrderLines"] = "An order needs at least one product.";
        if (modified.OrderLines.Any(l => l.Quantity < 1 || l.Quantity > MaxQuantityPerLine))
            errors["OrderLines"] = $"Quantities must be between 1 and {MaxQuantityPerLine}.";

        var productIds = modified.OrderLines.Select(l => l.ProductId).Distinct().ToList();
        var products = await dbContext.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Title, p.Price, p.Stock, p.IsActive })
            .ToDictionaryAsync(p => p.Id, token);

        // existing lines keep the price (and title) they were ordered at
        var existingLineIds = modified.OrderLines.Where(l => l.Id > 0).Select(l => l.Id).ToList();
        var storedLines = existingLineIds.Count > 0
            ? await dbContext.OrderLines.AsNoTracking()
                .Where(l => l.OrderId == modified.Id && existingLineIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, token)
            : [];

        foreach (var line in modified.OrderLines)
        {
            if (storedLines.TryGetValue(line.Id, out var storedLine) && storedLine.ProductId == line.ProductId)
            {
                line.UnitPrice = storedLine.UnitPrice;
                line.ProductTitle = storedLine.ProductTitle;
            }
            else if (products.TryGetValue(line.ProductId, out var product))
            {
                if (!product.IsActive)
                    errors["OrderLines"] = $"'{product.Title}' is no longer available.";
                else if (modified.Id == 0 && line.Quantity > product.Stock)
                    errors["OrderLines"] = product.Stock > 0
                        ? $"Only {product.Stock} x '{product.Title}' left in stock."
                        : $"'{product.Title}' is out of stock.";
                line.UnitPrice = product.Price;
                line.ProductTitle = product.Title;
            }
            else
            {
                errors["OrderLines"] = $"Product {line.ProductId} does not exist.";
            }
            line.LineTotal = Math.Round(line.UnitPrice * line.Quantity, 2);
        }

        if (errors.Count > 0)
        {
            var ex = new EntityInputException<Order>("Saving order failed");
            foreach (var (key, message) in errors) ex.InputErrors[key] = message;
            throw ex;
        }

        ApplyTotals(modified, modified.OrderLines.Sum(l => l.LineTotal), modified.OrderLines.Sum(l => l.Quantity));
    }

    private static void ApplyTotals(Order order, decimal subtotal, int itemCount)
    {
        order.Subtotal = subtotal;
        order.ItemCount = itemCount;
        order.ShippingCost = itemCount == 0 ? 0m : OrderPricing.ShippingCost(order.ShippingMethod, subtotal);
        order.Total = order.Subtotal + order.ShippingCost;
    }
}
