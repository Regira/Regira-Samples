using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Entities.Categories;

/// <summary>Create/update model for a category. Include <see cref="Id"/> to upsert via /save.</summary>
public class CategoryInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    /// <summary>Parent links (rows where this category is the child). Set <c>ParentId</c> per item.</summary>
    public ICollection<RelatedCategoryInputDto>? ParentEntities { get; set; }

    /// <summary>Child links (rows where this category is the parent). Set <c>ChildId</c> per item.</summary>
    public ICollection<RelatedCategoryInputDto>? ChildEntities { get; set; }
}

public class RelatedCategoryInputDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}
