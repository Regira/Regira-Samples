using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Entities.Lists;

/// <summary>Create/update model for a shopping list. Include <see cref="Id"/> to upsert via /save.</summary>
public class ShoppingListInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    public int ShopperId { get; set; }

    /// <summary>
    /// Full set of items on the list. Replacing this collection replaces the items (managed via
    /// Related()). For granular add/activate/deactivate use the items endpoints instead.
    /// </summary>
    public ICollection<ShoppingListItemInputDto>? Items { get; set; }
}

public class ShoppingListItemInputDto
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public int ArticleId { get; set; }
    public bool IsActive { get; set; } = true;
    public int Quantity { get; set; } = 1;

    [MaxLength(256)]
    public string? Note { get; set; }
}
