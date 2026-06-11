using ShoppingList.API.Entities.ShoppingListItems;

namespace ShoppingList.API.Entities.ShoppingLists;

/// <summary>Read model for a <see cref="ShoppingList"/>, including its items.</summary>
public class ShoppingListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int ShopperId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ShoppingListItemDto>? Items { get; set; }
}
