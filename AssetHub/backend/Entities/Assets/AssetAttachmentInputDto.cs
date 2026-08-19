using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.Assets;

public class AssetAttachmentInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(260)]
    public string FileName { get; set; } = null!;
    [MaxLength(100)]
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
}
