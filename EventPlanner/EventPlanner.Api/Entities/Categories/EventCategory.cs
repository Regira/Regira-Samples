using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace EventPlanner.Api.Entities.Categories;

public class EventCategory : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    /// <summary>Hex colour used for badges and banners, e.g. #ff4f81.</summary>
    [MaxLength(16)] public string? Color { get; set; }
    /// <summary>Icon name (Bootstrap Icons, without the "bi-" prefix).</summary>
    [MaxLength(48)] public string? Icon { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class EventCategoryDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class EventCategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    [MaxLength(48)] public string? Icon { get; set; }
}
