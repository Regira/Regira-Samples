namespace Webshop.Api.Entities.Promotions;

public class PromotionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string? Badge { get; set; }
    public string? CtaLabel { get; set; }
    public string? CtaLink { get; set; }
    public string? Theme { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class PromotionInputDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string? Badge { get; set; }
    public string? CtaLabel { get; set; }
    public string? CtaLink { get; set; }
    public string? Theme { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int SortOrder { get; set; }
}
