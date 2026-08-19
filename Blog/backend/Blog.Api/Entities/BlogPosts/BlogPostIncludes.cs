namespace Blog.Api.Entities.BlogPosts;

[Flags]
public enum BlogPostIncludes
{
    Default = 0,
    Tags = 1,
    All = Tags
}
