namespace AssetHub.Api.Entities.Assets;

public class AssetAttachmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string FileName { get; set; } = null!;
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public int SortOrder { get; set; }
}
