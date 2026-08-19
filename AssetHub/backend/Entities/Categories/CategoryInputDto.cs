using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
    [MaxLength(500)]
    public string? Description { get; set; }
}
