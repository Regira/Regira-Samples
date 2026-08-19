namespace EventPlanner.Api.Entities.Speakers;

public class SpeakerDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
