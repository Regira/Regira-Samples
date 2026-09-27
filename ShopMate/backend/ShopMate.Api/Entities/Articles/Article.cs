using System.ComponentModel.DataAnnotations;
using Regira.Entities.Attributes;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Entities.Articles;

/// <summary>
/// An article on a shopping list. <see cref="IsActive"/> = "still need to buy"; ticking it off deactivates it.
/// </summary>
public class Article : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, ISortable, IHasNormalizedContent
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public ShoppingList? ShoppingList { get; set; }

    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    [MaxLength(16)] public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Position in the list. Collection-level value: written only by POST /articles/reorder
    /// (and minted on create), so a PUT/PATCH of one article can never shuffle its siblings.
    /// </summary>
    [ServerOwned] public int SortOrder { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ArticleCategory>? Categories { get; set; }
}

/// <summary>Many-to-many join Article/Category (owned by the article, synced via Related()).</summary>
public class ArticleCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public Article? Article { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
