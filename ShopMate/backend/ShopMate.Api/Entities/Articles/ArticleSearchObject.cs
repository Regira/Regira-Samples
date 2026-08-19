using Regira.Entities.Models;

namespace ShopMate.Api.Entities.Articles;

public record ArticleSearchObject : SearchObject
{
    public ICollection<int>? ShoppingListId { get; set; }
    public ICollection<int>? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}
