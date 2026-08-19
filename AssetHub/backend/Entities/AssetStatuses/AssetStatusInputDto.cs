using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.AssetStatuses;

public class AssetStatusInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Title { get; set; } = null!;
    [Required, MaxLength(9)]
    public string ColorHex { get; set; } = "#64748b";
    public bool IsOperational { get; set; } = true;
    public int SortOrder { get; set; }
}
