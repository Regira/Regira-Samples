using System.ComponentModel.DataAnnotations;

namespace ShoppingList.API.Entities.Articles;

/// <summary>Create/update model for an <see cref="Article"/>.</summary>
public class ArticleInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(64)]
    public string? Brand { get; set; }

    [MaxLength(32)]
    public string? Unit { get; set; }

    public ICollection<ArticleCategoryInputDto>? Categories { get; set; }
}

public class ArticleCategoryInputDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
}
