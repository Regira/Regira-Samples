using System.ComponentModel.DataAnnotations;
using QCredits.Api.Entities.Employees;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace QCredits.Api.Entities.GroupTrainings;

public enum GroupTrainingStatus { Planned = 0, Completed = 1, Cancelled = 2 }

/// <summary>
/// A company-organised group training. Funded from a separate budget:
/// it never touches the personal QCredit balances of its participants.
/// </summary>
public class GroupTraining : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string? Title { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    [MaxLength(150)] public string? Provider { get; set; }
    [MaxLength(150)] public string? Location { get; set; }
    public DateOnly StartDate { get; set; }
    [Range(0.5, 30)] public double DurationDays { get; set; } = 1;
    /// <summary>Total cost in EUR, charged to the group-training budget.</summary>
    [Range(0, 1000000)] public decimal TotalCost { get; set; }
    public GroupTrainingStatus Status { get; set; } = GroupTrainingStatus.Planned;

    /// <summary>Number of participants (computed server-side).</summary>
    public int ParticipantCount { get; set; }
    public ICollection<GroupTrainingParticipant>? Participants { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Provider), nameof(Location)])]
    public string? NormalizedContent { get; set; }
}

/// <summary>Join row employee - group training (owned by the training).</summary>
public class GroupTrainingParticipant : IEntityWithSerial
{
    public int Id { get; set; }
    public int GroupTrainingId { get; set; }
    public GroupTraining? GroupTraining { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public bool Attended { get; set; }
}

public record GroupTrainingSearchObject : SearchObject
{
    public int? Year { get; set; }
    public ICollection<GroupTrainingStatus>? Status { get; set; }
    public ICollection<int>? EmployeeId { get; set; }
}

public enum GroupTrainingSortBy { Default = 0, StartDate, StartDateDesc, Title, ParticipantsDesc }

[Flags]
public enum GroupTrainingIncludes
{
    Default = 0,
    Participants = 1 << 0,
    All = Participants
}

public class GroupTrainingDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Provider { get; set; }
    public string? Location { get; set; }
    public DateOnly StartDate { get; set; }
    public double DurationDays { get; set; }
    public decimal TotalCost { get; set; }
    public GroupTrainingStatus Status { get; set; }
    public int ParticipantCount { get; set; }
    public ICollection<GroupTrainingParticipantDto>? Participants { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class GroupTrainingParticipantDto
{
    public int Id { get; set; }
    public int GroupTrainingId { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public bool Attended { get; set; }
}

public class GroupTrainingInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string? Title { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    [MaxLength(150)] public string? Provider { get; set; }
    [MaxLength(150)] public string? Location { get; set; }
    public DateOnly StartDate { get; set; }
    [Range(0.5, 30)] public double DurationDays { get; set; } = 1;
    [Range(0, 1000000)] public decimal TotalCost { get; set; }
    public GroupTrainingStatus Status { get; set; }
    public ICollection<GroupTrainingParticipantInputDto>? Participants { get; set; }
}

public class GroupTrainingParticipantInputDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public bool Attended { get; set; }
}
