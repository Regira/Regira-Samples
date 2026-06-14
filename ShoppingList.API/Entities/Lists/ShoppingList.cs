using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShoppingListApi.Entities.Shoppers;

namespace ShoppingListApi.Entities.Lists;

/// <summary>
/// A shopping list owned by a <see cref="Shopper"/>. Holds a collection of
/// <see cref="ShoppingListItem"/> entries that can be individually activated/deactivated.
/// </summary>
public class ShoppingList : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    public int ShopperId { get; set; }
    public Shopper? Shopper { get; set; }

    [MaxLength(256), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ShoppingListItem>? Items { get; set; }
}
