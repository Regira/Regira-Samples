using System.ComponentModel.DataAnnotations;

namespace ShoppingList.API.Entities.ShoppingListItems;

/// <summary>
/// Create/update model for a <see cref="ShoppingListItem"/>. Activating/deactivating an item is done
/// by sending <see cref="IsActive"/> (e.g. via <c>PATCH</c>).
/// </summary>
public class ShoppingListItemInputDto
{
    public int Id { get; set; }

    [Required]
    public int ShoppingListId { get; set; }

    [Required]
    public int ArticleId { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [MaxLength(256)]
    public string? Note { get; set; }
}
