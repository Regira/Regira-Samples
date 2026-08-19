using Regira.Entities.Models;

namespace AssetHub.Api.Entities.Locations;

public record LocationSearchObject : SearchObject
{
    public string? Title { get; set; }
}
