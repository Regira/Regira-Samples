namespace QCredits.Api.Entities.GroupTrainings;

public class GroupTrainingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime TrainingDate { get; set; }
    public string? Location { get; set; }
    public string? Facilitator { get; set; }
    public decimal Cost { get; set; }
    public int MaxParticipants { get; set; }
    public string? Department { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
