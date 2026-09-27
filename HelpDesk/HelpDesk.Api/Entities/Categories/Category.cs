using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Entities.SupportTeams;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace HelpDesk.Api.Entities.Categories;

public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    [MaxLength(32)] public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // routing: a new ticket in this category lands in this team's queue
    public int? SupportTeamId { get; set; }
    public SupportTeam? SupportTeam { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public record CategorySearchObject : SearchObject
{
    public ICollection<int>? SupportTeamId { get; set; }
    public bool? IsActive { get; set; }
}

public class CategoryCoreDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

public class CategoryDto : CategoryCoreDto
{
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int? SupportTeamId { get; set; }
    public SupportTeamCoreDto? SupportTeam { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    [MaxLength(32)] public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int? SupportTeamId { get; set; }
}
