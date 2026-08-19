using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPlanner.Api.Entities.EventCategories;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Sessions;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.Events;

public class Event : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(160)] public string Title { get; set; } = null!;
    [MaxLength(4096)] public string? Description { get; set; }
    [MaxLength(1024)] public string? BannerImageUrl { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public int EventCategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsFeatured { get; set; }

    // Independent entity with a back-ref collection — loaded via Include(), not owned via Related().
    public ICollection<Session>? Sessions { get; set; }

    // Filled by EventProcessor from Sessions / Registrations (cross-entity aggregates)
    [NotMapped] public int? SessionCount { get; set; }
    [NotMapped] public int? RegistrationCount { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
