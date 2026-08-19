using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Entities.Articles;

public class Article : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent, ISortable
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(512)] public string? Notes { get; set; }
    public decimal? Quantity { get; set; }
    [MaxLength(24)] public string? Unit { get; set; }

    /// <summary>Whether the shopper still needs to buy this article ("active" = on the to-buy list).</summary>
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public int ShoppingListId { get; set; }
    public ShoppingList? ShoppingList { get; set; }

    public ICollection<ArticleCategory>? Categories { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Notes)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
