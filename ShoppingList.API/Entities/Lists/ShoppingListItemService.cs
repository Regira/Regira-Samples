using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data;

namespace ShoppingListApi.Entities.Lists;

/// <summary>
/// Granular operations on individual shopping-list items: add an article, activate/deactivate it,
/// or remove it. Kept separate from the generic entity service so a shopper can toggle a single
/// article without resubmitting the whole list.
/// </summary>
public interface IShoppingListItemService
{
    Task<ShoppingListItem?> AddArticle(int listId, int articleId, int quantity = 1, string? note = null, CancellationToken token = default);
    Task<ShoppingListItem?> SetActive(int listId, int itemId, bool isActive, CancellationToken token = default);
    Task<bool> Remove(int listId, int itemId, CancellationToken token = default);
}

public class ShoppingListItemService(ShoppingListDbContext dbContext) : IShoppingListItemService
{
    public async Task<ShoppingListItem?> AddArticle(int listId, int articleId, int quantity = 1, string? note = null, CancellationToken token = default)
    {
        // Returns null when the list does not exist (→ 404). Unknown article is a client error (→ 400).
        if (!await dbContext.ShoppingLists.AnyAsync(x => x.Id == listId, token))
            return null;
        if (!await dbContext.Articles.AnyAsync(x => x.Id == articleId, token))
            throw new InvalidOperationException($"Article {articleId} does not exist.");

        // If the article is already on the list, (re)activate it and update quantity/note.
        var existing = await dbContext.ShoppingListItems
            .FirstOrDefaultAsync(x => x.ShoppingListId == listId && x.ArticleId == articleId, token);
        if (existing != null)
        {
            existing.IsActive = true;
            existing.Quantity = quantity < 1 ? 1 : quantity;
            if (note != null) existing.Note = note;
            await dbContext.SaveChangesAsync(token);
            return existing;
        }

        var maxSort = await dbContext.ShoppingListItems
            .Where(x => x.ShoppingListId == listId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(token) ?? 0;

        var item = new ShoppingListItem
        {
            ShoppingListId = listId,
            ArticleId = articleId,
            Quantity = quantity < 1 ? 1 : quantity,
            Note = note,
            IsActive = true,
            SortOrder = maxSort + 1
        };
        dbContext.ShoppingListItems.Add(item);
        await dbContext.SaveChangesAsync(token);
        return item;
    }

    public async Task<ShoppingListItem?> SetActive(int listId, int itemId, bool isActive, CancellationToken token = default)
    {
        var item = await dbContext.ShoppingListItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.ShoppingListId == listId, token);
        if (item == null)
            return null;

        item.IsActive = isActive;
        await dbContext.SaveChangesAsync(token);
        return item;
    }

    public async Task<bool> Remove(int listId, int itemId, CancellationToken token = default)
    {
        var item = await dbContext.ShoppingListItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.ShoppingListId == listId, token);
        if (item == null)
            return false;

        dbContext.ShoppingListItems.Remove(item);
        await dbContext.SaveChangesAsync(token);
        return true;
    }
}
