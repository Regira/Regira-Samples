using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using ShoppingListApi.Entities.Articles;

namespace ShoppingListApi.Entities.Lists;

/// <summary>
/// An <see cref="Article"/> placed on a <see cref="ShoppingList"/>. The <see cref="IsActive"/>
/// flag lets a shopper activate/deactivate the article without removing it from the list.
/// Owned and managed through the <see cref="ShoppingList"/> service via <c>Related()</c>.
/// </summary>
public class ShoppingListItem : IEntityWithSerial, ISortable
{
    public int Id { get; set; }

    public int ShoppingListId { get; set; }
    public ShoppingList? ShoppingList { get; set; }

    public int ArticleId { get; set; }
    public Article? Article { get; set; }

    /// <summary>Whether the article is currently active (to be bought) on the list.</summary>
    public bool IsActive { get; set; } = true;

    public int Quantity { get; set; } = 1;

    [MaxLength(256)]
    public string? Note { get; set; }

    public int SortOrder { get; set; }
}
