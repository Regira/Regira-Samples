using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Webshop.Api.Infrastructure;

public sealed class RoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefix = new(new RouteAttribute(prefix));

    public void Apply(ApplicationModel app)
    {
        foreach (var controller in app.Controllers)
            foreach (var selector in controller.Selectors)
                selector.AttributeRouteModel = selector.AttributeRouteModel is { } existing
                    ? AttributeRouteModel.CombineAttributeRouteModel(_prefix, existing)
                    : _prefix;
    }
}
