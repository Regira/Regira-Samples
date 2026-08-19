using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Entities.Tags;

public class TagInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(60)]
    public string Slug { get; set; } = null!;
}
