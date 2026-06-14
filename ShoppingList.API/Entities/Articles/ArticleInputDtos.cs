using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Entities.Articles;

/// <summary>Create/update model for an article. Include <see cref="Id"/> to upsert via /save.</summary>
public class ArticleInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(512)]
    public string? Description { get; set; }

    [MaxLength(64)]
    public string? Brand { get; set; }

    [MaxLength(16)]
    public string? Unit { get; set; }

    /// <summary>Category links. Set <c>CategoryId</c> per item; synced via Related().</summary>
    public ICollection<ArticleCategoryInputDto>? Categories { get; set; }
}

public class ArticleCategoryInputDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
}
