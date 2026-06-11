using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShoppingList.API.Entities.ShoppingListItems;

namespace ShoppingList.API.Entities.ShoppingLists;

/// <summary>
/// A named shopping list that belongs to a single <see cref="Shoppers.Shopper"/> and holds a
/// collection of <see cref="ShoppingListItem"/> entries (articles that can be activated/deactivated).
/// </summary>
public class ShoppingList : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    public int ShopperId { get; set; }
    public Shoppers.Shopper? Shopper { get; set; }

    [MaxLength(128), Normalized(SourceProperties = new[] { nameof(Name) })]
    public string? NormalizedContent { get; set; }

    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ShoppingListItem>? Items { get; set; }
}
