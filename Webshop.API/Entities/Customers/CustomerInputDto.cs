using System.ComponentModel.DataAnnotations;

namespace Webshop.API.Entities.Customers;

public class CustomerInputDto
{
    public Guid? Id { get; set; }
    [Required, MaxLength(256)] public string Name { get; set; } = null!;
    [Required, MaxLength(256), EmailAddress] public string Email { get; set; } = null!;
    [MaxLength(256)] public string? Phone { get; set; }
    [MaxLength(512)] public string? Address { get; set; }
}
