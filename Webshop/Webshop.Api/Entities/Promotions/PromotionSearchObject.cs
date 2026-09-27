using Regira.Entities.Models;

namespace Webshop.Api.Entities.Promotions;

public record PromotionSearchObject : SearchObject
{
    /// <summary>Only active promotions whose validity period contains today</summary>
    public bool? Live { get; set; }
}
