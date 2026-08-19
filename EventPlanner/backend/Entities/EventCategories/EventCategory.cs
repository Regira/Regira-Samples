using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.EventCategories;

public class EventCategory : IEntityWithSerial, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(16)] public string? ColorHex { get; set; }
    [MaxLength(64)] public string? Icon { get; set; }

    [MaxLength(256), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }
}
