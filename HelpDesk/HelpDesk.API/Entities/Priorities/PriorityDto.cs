namespace HelpDesk.API.Entities.Priorities;

public class PriorityCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int Level { get; set; }
    public string? ColorHex { get; set; }
}

public class PriorityDto : PriorityCoreDto
{
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
