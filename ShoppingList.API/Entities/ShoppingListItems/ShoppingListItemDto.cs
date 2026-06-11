using ShoppingList.API.Entities.Articles;

namespace ShoppingList.API.Entities.ShoppingListItems;

/// <summary>Read model for a <see cref="ShoppingListItem"/>, including the referenced article.</summary>
public class ShoppingListItemDto
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public int ArticleId { get; set; }
    public bool IsActive { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ArticleDto? Article { get; set; }
}
