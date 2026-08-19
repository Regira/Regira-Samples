using System.ComponentModel.DataAnnotations;

namespace HelpDesk.API.Entities.SupportTeams;

public class SupportTeamInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(512)] public string? Description { get; set; }
}
