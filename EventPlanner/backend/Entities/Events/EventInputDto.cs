using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Api.Entities.Events;

public class EventInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string Title { get; set; } = null!;
    [MaxLength(4096)] public string? Description { get; set; }
    [MaxLength(1024)] public string? BannerImageUrl { get; set; }
    public int LocationId { get; set; }
    public int EventCategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsFeatured { get; set; }
    // Sessions are independently addressable (own controller) — never on the parent's input DTO.
}
