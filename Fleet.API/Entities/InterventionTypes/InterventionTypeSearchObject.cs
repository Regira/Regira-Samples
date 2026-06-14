using Regira.Entities.Models;

namespace Fleet.API.Entities.InterventionTypes;

public record InterventionTypeSearchObject : SearchObject
{
    public string? Code { get; set; }
}
