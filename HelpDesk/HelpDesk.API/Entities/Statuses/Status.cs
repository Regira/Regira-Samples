using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace HelpDesk.API.Entities.Statuses;

public class Status : IEntityWithSerial, IHasTimestamps, IHasTitle
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string Title { get; set; } = null!;

    /// <summary>Kanban column order</summary>
    public int SortOrder { get; set; }

    /// <summary>Marks a status as a "done" state (Resolved/Closed) - excluded from open-ticket counts</summary>
    public bool IsClosed { get; set; }
    [MaxLength(7)] public string? ColorHex { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
