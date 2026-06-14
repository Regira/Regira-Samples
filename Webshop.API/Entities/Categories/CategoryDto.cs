namespace Webshop.API.Entities.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
