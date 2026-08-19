using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.Employees;

public class Employee : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(80)]
    public string FirstName { get; set; } = null!;
    [Required, MaxLength(80)]
    public string LastName { get; set; } = null!;
    [Required, MaxLength(150)]
    public string Email { get; set; } = null!;
    [MaxLength(100)]
    public string? Department { get; set; }
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
