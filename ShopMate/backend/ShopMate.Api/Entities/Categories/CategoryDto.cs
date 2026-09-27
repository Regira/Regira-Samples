namespace ShopMate.Api.Entities.Categories;

public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public class CategoryDto : CategoryCoreDto
{
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ParentCategoryDto>? ParentEntities { get; set; }
    public ICollection<ChildCategoryDto>? ChildEntities { get; set; }
    public int? ArticleCount { get; set; }
}

public class RelatedCategoryDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
}
public class ParentCategoryDto : RelatedCategoryDto { public CategoryCoreDto? Parent { get; set; } }
public class ChildCategoryDto : RelatedCategoryDto { public CategoryCoreDto? Child { get; set; } }

public class CategoryInputDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    // nullable + uninitialized: null = leave the rows untouched, [] = remove all
    public ICollection<RelatedCategoryInputDto>? ParentEntities { get; set; }
    public ICollection<RelatedCategoryInputDto>? ChildEntities { get; set; }
}

public class RelatedCategoryInputDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
}
