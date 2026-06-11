using ShoppingList.API.Entities.Categories;

namespace ShoppingList.API.Entities.Articles;

/// <summary>Read model for an <see cref="Article"/>, optionally including its categories.</summary>
public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Unit { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ArticleCategoryDto>? Categories { get; set; }
}

public class ArticleCategoryDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}
