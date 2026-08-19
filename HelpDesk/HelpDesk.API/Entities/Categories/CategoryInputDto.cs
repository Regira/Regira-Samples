using System.ComponentModel.DataAnnotations;

namespace HelpDesk.API.Entities.Categories;

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(7)] public string? ColorHex { get; set; }
}
