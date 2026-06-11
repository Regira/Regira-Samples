using Regira.Entities.Models;

namespace Webshop.API.Entities.Products;

public record ProductSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
}
