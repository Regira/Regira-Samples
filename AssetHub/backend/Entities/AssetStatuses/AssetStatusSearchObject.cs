using Regira.Entities.Models;

namespace AssetHub.Api.Entities.AssetStatuses;

public record AssetStatusSearchObject : SearchObject
{
    public string? Title { get; set; }
}
