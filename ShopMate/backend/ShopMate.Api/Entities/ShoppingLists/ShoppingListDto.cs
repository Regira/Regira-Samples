namespace ShopMate.Api.Entities.ShoppingLists;

/// <summary>Slim shape used as the nested reference on Article (avoids re-computing counts per row).</summary>
public class ShoppingListCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? OwnerName { get; set; }
    public string? ColorHex { get; set; }
    public string? Icon { get; set; }
}

public class ShoppingListDto : ShoppingListCoreDto
{
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? ArticleCount { get; set; }
    public int? ActiveArticleCount { get; set; }
}
