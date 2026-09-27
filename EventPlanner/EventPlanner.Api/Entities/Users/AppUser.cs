using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EventPlanner.Api.Entities.Users;

/// <summary>An employee account (ASP.NET Identity). Administrators carry the "Admin" role.</summary>
public class AppUser : IdentityUser
{
    [MaxLength(64)] public string? FirstName { get; set; }
    [MaxLength(64)] public string? LastName { get; set; }
    [MaxLength(64)] public string? Department { get; set; }
    [MaxLength(96)] public string? JobTitle { get; set; }
}

/// <summary>Slim projection of an employee — never exposes Identity internals (hashes, stamps).</summary>
public class EmployeeDto
{
    public string Id { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
}
