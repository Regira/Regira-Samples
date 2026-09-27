using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Entities.Persons;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace HelpDesk.Api.Entities.SupportTeams;

public class SupportTeam : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(128)] public string? Email { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Email)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    // back-reference: members are written through Person.SupportTeamId, never through the team
    public ICollection<Person>? Members { get; set; }
}

public class SupportTeamCoreDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Color { get; set; }
}

public class SupportTeamDto : SupportTeamCoreDto
{
    public string? Description { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<PersonCoreDto>? Members { get; set; }
}

public class SupportTeamInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(128)] public string? Email { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    public bool IsActive { get; set; } = true;
}
