using System.ComponentModel.DataAnnotations;

namespace ShopMate.Api.Entities.Articles;

public class ArticleInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(512)] public string? Notes { get; set; }
    public decimal? Quantity { get; set; }
    [MaxLength(24)] public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int ShoppingListId { get; set; }
    public ICollection<ArticleCategoryInputDto>? Categories { get; set; }
}

public class ArticleCategoryInputDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }
}
