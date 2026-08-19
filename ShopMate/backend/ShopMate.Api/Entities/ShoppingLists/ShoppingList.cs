using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;

namespace ShopMate.Api.Entities.ShoppingLists;

public class ShoppingList : IEntityWithSerial, IHasTimestamps, IHasTitle, IArchivable
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(64)] public string? OwnerName { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(16)] public string? ColorHex { get; set; }
    [MaxLength(32)] public string? Icon { get; set; }
    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [NotMapped] public int? ArticleCount { get; set; }
    [NotMapped] public int? ActiveArticleCount { get; set; }
}
