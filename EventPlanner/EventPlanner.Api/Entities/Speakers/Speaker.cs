using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.Speakers;

public class Speaker : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? FirstName { get; set; }
    [Required, MaxLength(64)] public string? LastName { get; set; }
    [MaxLength(128)] public string? Company { get; set; }
    [MaxLength(128)] public string? JobTitle { get; set; }
    [MaxLength(4000)] public string? Bio { get; set; }
    [MaxLength(256)] public string? Email { get; set; }
    [MaxLength(512)] public string? PhotoUrl { get; set; }
    [MaxLength(256)] public string? Topics { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(FirstName), nameof(LastName), nameof(Company), nameof(JobTitle), nameof(Topics)])]
    public string? NormalizedContent { get; set; }
}

public record SpeakerSearchObject : SearchObject
{
    public ICollection<string>? Company { get; set; }
}

public class SpeakerDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Bio { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Topics { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class SpeakerInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? FirstName { get; set; }
    [Required, MaxLength(64)] public string? LastName { get; set; }
    [MaxLength(128)] public string? Company { get; set; }
    [MaxLength(128)] public string? JobTitle { get; set; }
    [MaxLength(4000)] public string? Bio { get; set; }
    [MaxLength(256), EmailAddress] public string? Email { get; set; }
    [MaxLength(512)] public string? PhotoUrl { get; set; }
    [MaxLength(256)] public string? Topics { get; set; }
}
