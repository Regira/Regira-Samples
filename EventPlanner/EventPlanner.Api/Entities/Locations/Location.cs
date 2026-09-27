using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.Locations;

public class Location : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(2048)] public string? Description { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(96)] public string? City { get; set; }
    [MaxLength(96)] public string? Country { get; set; }
    public int Capacity { get; set; }
    [MaxLength(512)] public string? ImageUrl { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(City), nameof(Address), nameof(Country)])]
    public string? NormalizedContent { get; set; }
}

public record LocationSearchObject : SearchObject
{
    public ICollection<string>? City { get; set; }
    public int? MinCapacity { get; set; }
}

public class LocationDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int Capacity { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class LocationInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(2048)] public string? Description { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(96)] public string? City { get; set; }
    [MaxLength(96)] public string? Country { get; set; }
    [Range(0, 100000)] public int Capacity { get; set; }
    [MaxLength(512)] public string? ImageUrl { get; set; }
}
