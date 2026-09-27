using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Entities.Tags;

public class TagInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(48)] public string Title { get; set; } = null!;
    /// <summary>Optional - generated from the title when left empty.</summary>
    [MaxLength(64)] public string? Slug { get; set; }
}
