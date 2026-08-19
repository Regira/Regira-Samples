using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace HelpDesk.API.Entities.Categories;

public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(7)] public string? ColorHex { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
