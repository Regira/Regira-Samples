using Regira.Entities.Models;

namespace Webshop.Api.Entities.Categories;

public record CategorySearchObject : SearchObject
{
    public bool? IsFeatured { get; set; }
}
