using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace HelpDesk.API.Entities.Priorities;

public class Priority : IEntityWithSerial, IHasTimestamps, IHasTitle
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string Title { get; set; } = null!;

    /// <summary>Ordering weight, higher = more urgent (e.g. Low=1 .. Urgent=4)</summary>
    public int Level { get; set; }
    [MaxLength(7)] public string? ColorHex { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
