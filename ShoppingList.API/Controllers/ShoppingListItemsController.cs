using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Entities.Lists;

namespace ShoppingListApi.Controllers;

/// <summary>
/// Granular operations on the items of a shopping list. This is how a shopper
/// activates/deactivates individual articles without resubmitting the whole list.
/// </summary>
[ApiController, Route("shoppinglists/{listId:int}/items")]
public class ShoppingListItemsController(IShoppingListItemService itemService) : ControllerBase
{
    /// <summary>Add an article to the list (re-activates it if already present).</summary>
    [HttpPost]
    public async Task<IActionResult> Add(int listId, [FromBody] AddItemRequest request, CancellationToken token)
    {
        try
        {
            var item = await itemService.AddArticle(listId, request.ArticleId, request.Quantity, request.Note, token);
            return item is null ? NotFound() : Ok(ToDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Activate an article on the list.</summary>
    [HttpPost("{itemId:int}/activate")]
    public Task<IActionResult> Activate(int listId, int itemId, CancellationToken token)
        => SetActive(listId, itemId, true, token);

    /// <summary>Deactivate an article on the list (keeps it on the list).</summary>
    [HttpPost("{itemId:int}/deactivate")]
    public Task<IActionResult> Deactivate(int listId, int itemId, CancellationToken token)
        => SetActive(listId, itemId, false, token);

    /// <summary>Remove an article from the list entirely.</summary>
    [HttpDelete("{itemId:int}")]
    public async Task<IActionResult> Remove(int listId, int itemId, CancellationToken token)
    {
        var removed = await itemService.Remove(listId, itemId, token);
        return removed ? NoContent() : NotFound();
    }

    private async Task<IActionResult> SetActive(int listId, int itemId, bool isActive, CancellationToken token)
    {
        var item = await itemService.SetActive(listId, itemId, isActive, token);
        return item is null ? NotFound() : Ok(ToDto(item));
    }

    private static object ToDto(ShoppingListItem item) => new
    {
        item.Id,
        item.ShoppingListId,
        item.ArticleId,
        item.IsActive,
        item.Quantity,
        item.Note,
        item.SortOrder
    };

    public record AddItemRequest(int ArticleId, int Quantity = 1, string? Note = null);
}
