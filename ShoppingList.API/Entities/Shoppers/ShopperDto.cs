namespace ShoppingList.API.Entities.Shoppers;

/// <summary>Read model for a <see cref="Shopper"/>.</summary>
public class ShopperDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
