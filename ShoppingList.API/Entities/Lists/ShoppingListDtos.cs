using ShoppingListApi.Entities.Articles;
using ShoppingListApi.Entities.Shoppers;

namespace ShoppingListApi.Entities.Lists;

public class ShoppingListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ShopperId { get; set; }
    public ShopperDto? Shopper { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Total number of items on the list (computed in an after-mapper).</summary>
    public int ItemCount { get; set; }

    /// <summary>Number of currently active items (computed in an after-mapper).</summary>
    public int ActiveItemCount { get; set; }

    public ICollection<ShoppingListItemDto>? Items { get; set; }
}

public class ShoppingListItemDto
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public int ArticleId { get; set; }
    public ArticleDto? Article { get; set; }
    public bool IsActive { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
