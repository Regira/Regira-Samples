using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.Locations;

public class Location : IEntityWithSerial, IHasTitle, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
    [MaxLength(150)]
    public string? Building { get; set; }
    [MaxLength(50)]
    public string? Room { get; set; }
    [MaxLength(250)]
    public string? Address { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
