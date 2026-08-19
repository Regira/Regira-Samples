namespace Webshop.Api.Entities.Categories;

public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
}

public class CategoryDto : CategoryCoreDto
{
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public int? ProductCount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
