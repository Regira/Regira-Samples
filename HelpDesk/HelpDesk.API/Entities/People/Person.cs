using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using HelpDesk.API.Entities.SupportTeams;

namespace HelpDesk.API.Entities.People;

/// <summary>
/// Role-discriminated actor (Customer / Agent / Admin) - see Regira.Entities "Stakeholders" remedy:
/// one Person entity instead of separate Customer/Employee registrations keeps the free-tier budget intact
/// while still modelling that a human is only ever one of several roles.
/// </summary>
public class Person : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string FullName { get; set; } = null!;
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(32)] public string? Phone { get; set; }
    public PersonRole Role { get; set; }

    /// <summary>Customers only</summary>
    [MaxLength(128)] public string? Company { get; set; }

    /// <summary>Agents only</summary>
    [MaxLength(128)] public string? JobTitle { get; set; }

    /// <summary>Agents only - the support team they belong to</summary>
    public int? SupportTeamId { get; set; }
    public SupportTeam? SupportTeam { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(FullName), nameof(Email), nameof(Company)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [NotMapped] public int? AssignedTicketCount { get; set; }
}
