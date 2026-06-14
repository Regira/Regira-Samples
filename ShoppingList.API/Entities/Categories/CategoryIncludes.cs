namespace ShoppingListApi.Entities.Categories;

/// <summary>Navigation properties that can be eager-loaded for a <see cref="Category"/>.</summary>
[Flags]
public enum CategoryIncludes
{
    Default = 0,
    Parents = 1 << 0,
    Children = 1 << 1,
    All = Parents | Children
}
