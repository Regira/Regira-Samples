namespace AssetHub.Api.Entities.AssetStatuses;

public class AssetStatusDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string ColorHex { get; set; } = null!;
    public bool IsOperational { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
