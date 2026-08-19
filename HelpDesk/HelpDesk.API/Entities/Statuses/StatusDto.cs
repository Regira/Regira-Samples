namespace HelpDesk.API.Entities.Statuses;

public class StatusCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsClosed { get; set; }
    public string? ColorHex { get; set; }
}

public class StatusDto : StatusCoreDto
{
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
