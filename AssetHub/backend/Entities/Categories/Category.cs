using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.Categories;

public class Category : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
    [MaxLength(500)]
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
