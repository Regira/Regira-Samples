namespace EventPlanner.Api.Entities.EventCategories;

public class EventCategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ColorHex { get; set; }
    public string? Icon { get; set; }
}
