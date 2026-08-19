using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.Locations;

public class Location : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    [Required, MaxLength(256)] public string Address { get; set; } = null!;
    [Required, MaxLength(128)] public string City { get; set; } = null!;
    [MaxLength(32)] public string? PostalCode { get; set; }
    [Required, MaxLength(128)] public string Country { get; set; } = null!;
    public int Capacity { get; set; }
    [MaxLength(1024)] public string? ImageUrl { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(City), nameof(Country)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
