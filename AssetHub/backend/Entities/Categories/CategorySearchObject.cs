using Regira.Entities.Models;

namespace AssetHub.Api.Entities.Categories;

public record CategorySearchObject : SearchObject
{
    public string? Title { get; set; }
}
