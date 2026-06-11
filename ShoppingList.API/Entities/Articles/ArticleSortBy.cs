namespace ShoppingList.API.Entities.Articles;

/// <summary>Sort options for <see cref="Article"/> list/search results.</summary>
public enum ArticleSortBy
{
    Default = 0,
    Title,
    TitleDesc,
    Newest,
    Oldest
}
