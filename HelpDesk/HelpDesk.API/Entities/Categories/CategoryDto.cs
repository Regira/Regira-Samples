namespace HelpDesk.API.Entities.Categories;

public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ColorHex { get; set; }
}

public class CategoryDto : CategoryCoreDto
{
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
