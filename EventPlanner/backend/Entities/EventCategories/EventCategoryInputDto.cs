using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Api.Entities.EventCategories;

public class EventCategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(16)] public string? ColorHex { get; set; }
    [MaxLength(64)] public string? Icon { get; set; }
}
