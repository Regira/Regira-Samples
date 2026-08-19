namespace ShopMate.Api.Entities.Articles;

[Flags]
public enum ArticleIncludes
{
    Default = 0,
    Categories = 1,
    All = Categories
}
