namespace AssetHub.Api.Entities.Suppliers;

public class SupplierDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
