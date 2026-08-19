using System.ComponentModel.DataAnnotations;

namespace HelpDesk.API.Entities.Statuses;

public class StatusInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string Title { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsClosed { get; set; }
    [MaxLength(7)] public string? ColorHex { get; set; }
}
