using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Api.Entities.Speakers;

public class SpeakerInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    [MaxLength(128)] public string? JobTitle { get; set; }
    [MaxLength(128)] public string? Company { get; set; }
    [MaxLength(256)] public string? Email { get; set; }
    [MaxLength(1024)] public string? PhotoUrl { get; set; }
}
