using System.ComponentModel.DataAnnotations;

namespace HelpDesk.API.Entities.People;

public class PersonInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string FullName { get; set; } = null!;
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(32)] public string? Phone { get; set; }
    public PersonRole Role { get; set; }
    [MaxLength(128)] public string? Company { get; set; }
    [MaxLength(128)] public string? JobTitle { get; set; }
    public int? SupportTeamId { get; set; }
    public bool IsActive { get; set; } = true;
}
