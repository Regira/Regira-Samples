using System.ComponentModel.DataAnnotations;

namespace Webshop.Api.Entities.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;
    [MaxLength(64)]
    public string? Slug { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }
    [MaxLength(512)]
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
}
