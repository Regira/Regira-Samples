using System.ComponentModel.DataAnnotations;

namespace Webshop.API.Entities.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public bool IsArchived { get; set; }
}
