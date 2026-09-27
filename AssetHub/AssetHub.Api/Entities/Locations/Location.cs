using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace AssetHub.Api.Entities.Locations;

public class Location : IEntityWithSerial, IHasTitle, IHasCode, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(16)] public string? Code { get; set; }
    [MaxLength(128)] public string? Address { get; set; }
    [MaxLength(64)] public string? City { get; set; }
    [MaxLength(64)] public string? Country { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Code), nameof(City), nameof(Country)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public record LocationSearchObject : SearchObject;

public class LocationDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class LocationInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(16)] public string? Code { get; set; }
    [MaxLength(128)] public string? Address { get; set; }
    [MaxLength(64)] public string? City { get; set; }
    [MaxLength(64)] public string? Country { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
}
