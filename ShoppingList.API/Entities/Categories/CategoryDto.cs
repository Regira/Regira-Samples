namespace ShoppingList.API.Entities.Categories;

/// <summary>Minimal category projection, reused inside parent/child links to avoid deep nesting.</summary>
public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? ArticleCount { get; set; }
}

/// <summary>Full category read model, optionally including parent/child links.</summary>
public class CategoryDto : CategoryCoreDto
{
    public ICollection<ParentCategoryDto>? ParentEntities { get; set; }
    public ICollection<ChildCategoryDto>? ChildEntities { get; set; }
}

public class RelatedCategoryDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
}

public class ParentCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Parent { get; set; } = null!;
}

public class ChildCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Child { get; set; } = null!;
}
