namespace Blog.Api.Entities.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? PostCount { get; set; }
    public int? PublishedPostCount { get; set; }
}
