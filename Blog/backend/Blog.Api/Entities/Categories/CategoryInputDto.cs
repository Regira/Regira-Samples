using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Entities.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(120)]
    public string Slug { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }
}
