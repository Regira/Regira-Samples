using Regira.Entities.Models.Abstractions;
using ShoppingListApi.Entities.Categories;

namespace ShoppingListApi.Entities.Articles;

/// <summary>
/// Many-to-many join between an <see cref="Article"/> and a <see cref="Category"/>. Owned and
/// managed through the <see cref="Article"/> service via <c>Related()</c>.
/// </summary>
public class ArticleCategory : IEntityWithSerial
{
    public int Id { get; set; }

    public int ArticleId { get; set; }
    public Article? Article { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
