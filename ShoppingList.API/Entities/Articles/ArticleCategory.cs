using Regira.Entities.Models.Abstractions;
using ShoppingList.API.Entities.Categories;

namespace ShoppingList.API.Entities.Articles;

/// <summary>
/// Many-to-many join entity between <see cref="Article"/> and <see cref="Category"/>.
/// Managed as an owned collection of <see cref="Article"/> via <c>e.Related(...)</c>.
/// </summary>
public class ArticleCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public Article? Article { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
