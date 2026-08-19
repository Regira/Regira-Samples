using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Entities.Articles;

public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Notes { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int ShoppingListId { get; set; }
    public ShoppingListCoreDto? ShoppingList { get; set; }
    public ICollection<ArticleCategoryDto>? Categories { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class ArticleCategoryDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}
