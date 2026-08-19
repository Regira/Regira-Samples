using System.ComponentModel.DataAnnotations;

namespace HelpDesk.API.Entities.Priorities;

public class PriorityInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string Title { get; set; } = null!;
    public int Level { get; set; }
    [MaxLength(7)] public string? ColorHex { get; set; }
}
