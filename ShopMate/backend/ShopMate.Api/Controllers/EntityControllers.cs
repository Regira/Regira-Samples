using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.Controllers.Abstractions;
using ShopMate.Api.Data;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.Shoppers;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Controllers;

[ApiController, Route("shoppers")]
public class ShopperController : EntityControllerBase<Shopper, ShopperDto, ShopperInputDto>;

[ApiController, Route("shopping-lists")]
public class ShoppingListController : EntityControllerBase<ShoppingList, int, ShoppingListSearchObject, ShoppingListDto, ShoppingListInputDto>;

[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;

[ApiController, Route("articles")]
public class ArticleController : EntityControllerBase<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes, ArticleDto, ArticleInputDto>
{
    /// <summary>
    /// Writes the collection-level SortOrder of one list in a single call: <c>ids</c> is the complete
    /// new order (ids not belonging to the list are ignored; list articles missing from <c>ids</c> keep
    /// their relative order after the given ones).
    /// </summary>
    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ArticleReorderInput input, [FromServices] ShopMateDbContext db, CancellationToken token)
    {
        var articles = await db.Articles
            .Where(x => x.ShoppingListId == input.ShoppingListId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync(token);
        if (articles.Count == 0)
            return NotFound();

        var position = new Dictionary<int, int>();
        for (var i = 0; i < input.Ids.Count; i++)
            position.TryAdd(input.Ids[i], i);

        var ordered = articles
            .Select((a, i) => (Article: a, Key: position.TryGetValue(a.Id, out var p) ? p : input.Ids.Count + i))
            .OrderBy(x => x.Key)
            .Select(x => x.Article)
            .ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SortOrder = i + 1;

        await db.SaveChangesAsync(token);
        return Ok(new { items = ordered.Select(a => new { a.Id, a.SortOrder }) });
    }
}
