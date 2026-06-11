namespace Webshop.API.Entities.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
