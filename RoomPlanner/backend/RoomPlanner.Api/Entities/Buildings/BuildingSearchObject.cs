using Regira.Entities.Models;

namespace RoomPlanner.Api.Entities.Buildings;

public record BuildingSearchObject : SearchObject
{
    public string? City { get; set; }
}
