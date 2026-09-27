using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Shoppers;

namespace ShopMate.Api.Entities.ShoppingLists;

public class ShoppingList : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    public int ShopperId { get; set; }
    public Shopper? Shopper { get; set; }

    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    /// <summary>Hex colour used for the list card in the SPA.</summary>
    [MaxLength(16)] public string? Color { get; set; }
    /// <summary>Pinned lists float to the top of the overview.</summary>
    public bool IsPinned { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<Article>? Articles { get; set; }

    // filled by ShoppingListProcessor
    [NotMapped] public int? ArticleCount { get; set; }
    [NotMapped] public int? ActiveCount { get; set; }
}
