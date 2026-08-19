using System.ComponentModel.DataAnnotations;

namespace ShopMate.Api.Entities.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(32)] public string? Icon { get; set; }
    [MaxLength(16)] public string? ColorHex { get; set; }
    public ICollection<RelatedCategoryInputDto>? ParentEntities { get; set; }
    public ICollection<RelatedCategoryInputDto>? ChildEntities { get; set; }
}

public class RelatedCategoryInputDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
}
