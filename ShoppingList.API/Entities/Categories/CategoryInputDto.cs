using System.ComponentModel.DataAnnotations;

namespace ShoppingList.API.Entities.Categories;

/// <summary>Create/update model for a <see cref="Category"/>.</summary>
public class CategoryInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public ICollection<RelatedCategoryInputDto>? ParentEntities { get; set; }
    public ICollection<RelatedCategoryInputDto>? ChildEntities { get; set; }
}

public class RelatedCategoryInputDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}
