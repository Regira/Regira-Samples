namespace ShopMate.Api.Entities.Categories;

public class CategoryCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Icon { get; set; }
    public string? ColorHex { get; set; }
}

public class CategoryDto : CategoryCoreDto
{
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

public class ParentCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Parent { get; set; } = null!;
}

public class ChildCategoryDto : RelatedCategoryDto
{
    public CategoryCoreDto Child { get; set; } = null!;
}
