using Microsoft.AspNetCore.Routing;
using Shawarma.AspNetCore.Internal;

namespace Shawarma.AspNetCore;

/// <summary>
/// Options for controlling Shawarma behaviors.
/// </summary>
public class ShawarmaOptions
{
    /// <summary>
    /// Route template intercepted by Shawarma.
    /// </summary>
    public string RouteTemplate { get; set; } = ShawarmaConstants.DefaultRouteTemplate;

    /// <summary>
    /// Default route values.
    /// </summary>
    public RouteValueDictionary? RouteDefaults { get; set; }
}
