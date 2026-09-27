using System.ComponentModel.DataAnnotations;

namespace RoomPlanner.Api.Entities.Buildings;

public class BuildingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public TimeOnly OpensAt { get; set; }
    public TimeOnly ClosesAt { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class BuildingInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(16)] public string? Code { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(128)] public string? City { get; set; }
    public TimeOnly OpensAt { get; set; } = new(7, 0);
    public TimeOnly ClosesAt { get; set; } = new(20, 0);
}
