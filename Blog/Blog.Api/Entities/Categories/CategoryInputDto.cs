using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Entities.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    /// <summary>Optional - generated from the title when left empty.</summary>
    [MaxLength(128)] public string? Slug { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    public int SortOrder { get; set; }
}
