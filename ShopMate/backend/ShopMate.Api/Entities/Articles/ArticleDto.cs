using ShopMate.Api.Entities.Categories;

namespace ShopMate.Api.Entities.Articles;

public class ArticleDto
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public ArticleShoppingListDto? ShoppingList { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ArticleCategoryDto>? Categories { get; set; }
}

public class ArticleShoppingListDto
{
    public int Id { get; set; }
    public int ShopperId { get; set; }
    public string Title { get; set; } = null!;
    public string? Color { get; set; }
}

public class ArticleCategoryDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}

public class ArticleInputDto
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
    // SortOrder is server-owned: reorder through POST /articles/reorder
    // nullable + uninitialized: null = leave the rows untouched, [] = remove all
    public ICollection<ArticleCategoryInputDto>? Categories { get; set; }
}

public class ArticleCategoryInputDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
}

/// <summary>Body of POST /articles/reorder: the article ids of one list in their new order.</summary>
public class ArticleReorderInput
{
    public int ShoppingListId { get; set; }
    public List<int> Ids { get; set; } = [];
}
