using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HelpDesk.Api.Entities.SupportTeams;
using Regira.Entities.Attributes;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace HelpDesk.Api.Entities.Persons;

public enum PersonRole
{
    Customer = 0,
    Employee = 1
}

/// <summary>
/// One role-discriminated actor for customers and employees (the budget-friendly "flat contact" shape):
/// both appear on tickets and comments, and share name/email/phone.
/// </summary>
public class Person : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    public PersonRole Role { get; set; }
    [Required, MaxLength(64)] public string? GivenName { get; set; }
    [Required, MaxLength(64)] public string? FamilyName { get; set; }
    [MaxLength(128)] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    /// <summary>Customers: their organisation.</summary>
    [MaxLength(128)] public string? Company { get; set; }
    /// <summary>Employees: their function.</summary>
    [MaxLength(64)] public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Employees: the team whose queue they work.</summary>
    public int? SupportTeamId { get; set; }
    public SupportTeam? SupportTeam { get; set; }

    /// <summary>Link to the Identity account (AspNetUsers.Id); written by the account provisioning only.</summary>
    [ServerOwned, MaxLength(450)] public string? UserId { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(GivenName), nameof(FamilyName), nameof(Email), nameof(Company)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [NotMapped] public string FullName => $"{GivenName} {FamilyName}".Trim();
    [NotMapped] public bool HasAccount => !string.IsNullOrEmpty(UserId);
}

public record PersonSearchObject : SearchObject
{
    public ICollection<PersonRole>? Role { get; set; }
    public ICollection<int>? SupportTeamId { get; set; }
    public bool? HasAccount { get; set; }
    public bool? IsActive { get; set; }
}

public enum PersonSortBy
{
    Default = 0,
    Name,
    NameDesc,
    Company,
    Newest
}

/// <summary>Summary shape nested in tickets and comments.</summary>
public class PersonCoreDto
{
    public int Id { get; set; }
    public PersonRole Role { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
}

public class PersonDto : PersonCoreDto
{
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public int? SupportTeamId { get; set; }
    public SupportTeamCoreDto? SupportTeam { get; set; }
    public bool HasAccount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class PersonInputDto
{
    public int Id { get; set; }
    public PersonRole Role { get; set; }
    [Required, MaxLength(64)] public string? GivenName { get; set; }
    [Required, MaxLength(64)] public string? FamilyName { get; set; }
    [MaxLength(128), EmailAddress] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    [MaxLength(128)] public string? Company { get; set; }
    [MaxLength(64)] public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
    public int? SupportTeamId { get; set; }
}
