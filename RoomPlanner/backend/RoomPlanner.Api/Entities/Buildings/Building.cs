using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace RoomPlanner.Api.Entities.Buildings;

public class Building : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(512)]
    public string? Description { get; set; }

    [Required, MaxLength(256)]
    public string Address { get; set; } = null!;

    [Required, MaxLength(64)]
    public string City { get; set; } = null!;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Address), nameof(City)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
