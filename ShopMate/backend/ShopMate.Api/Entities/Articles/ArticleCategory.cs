using Regira.Entities.Models.Abstractions;
using ShopMate.Api.Entities.Categories;

namespace ShopMate.Api.Entities.Articles;

/// <summary>Many-to-many join row: an article can carry several categories.</summary>
public class ArticleCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public Article? Article { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
