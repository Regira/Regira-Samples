namespace ShoppingListApi.Entities.Categories;

/// <summary>Lightweight category projection used both standalone and as a nested parent/child.</summary>
public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>Full category projection including its parent and child relationships.</summary>
public class CategoryDto : CategoryCoreDto
{
    public int? ArticleCount { get; set; }
    public ICollection<ParentCategoryDto>? ParentEntities { get; set; }
    public ICollection<ChildCategoryDto>? ChildEntities { get; set; }
}

public class RelatedCategoryDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}

/// <summary>A relationship seen from the child: exposes the <see cref="Parent"/> category.</summary>
public class ParentCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Parent { get; set; } = null!;
}

/// <summary>A relationship seen from the parent: exposes the <see cref="Child"/> category.</summary>
public class ChildCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Child { get; set; } = null!;
}
