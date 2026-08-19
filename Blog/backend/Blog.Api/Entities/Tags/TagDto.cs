namespace Blog.Api.Entities.Tags;

public class TagDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
