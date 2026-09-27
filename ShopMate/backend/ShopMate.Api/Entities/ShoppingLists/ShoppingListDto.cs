using ShopMate.Api.Entities.Shoppers;

namespace ShopMate.Api.Entities.ShoppingLists;

public class ShoppingListDto
{
    public int Id { get; set; }
    public int ShopperId { get; set; }
    public ShopperDto? Shopper { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsPinned { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? ArticleCount { get; set; }
    public int? ActiveCount { get; set; }
}

public class ShoppingListInputDto
{
    public int Id { get; set; }
    public int ShopperId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsPinned { get; set; }
    // Articles are deliberately absent: they are written through their own /articles endpoints
    // (one writer per save path).
}
