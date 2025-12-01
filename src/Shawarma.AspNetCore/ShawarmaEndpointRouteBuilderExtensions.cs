using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Shawarma.AspNetCore.Internal;

namespace Shawarma.AspNetCore;

/// <summary>
/// Shawarma extensions for <see cref="IEndpointRouteBuilder" />
/// </summary>
public static class ShawarmaExtensions
{
    /// <summary>
    /// Register Shawarma on the default route.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> for further customization.</returns>
    public static IEndpointConventionBuilder MapShawarma(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapShawarma(RoutePatternFactory.Parse(ShawarmaConstants.DefaultRouteTemplate));

    /// <summary>
    /// Register Shawarma on a custom route.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The custom route.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> for further customization.</returns>
    public static IEndpointConventionBuilder MapShawarma(this IEndpointRouteBuilder endpoints, string pattern) =>
        endpoints.MapShawarma(RoutePatternFactory.Parse(pattern));

    /// <summary>
    /// Register Shawarma on a custom route.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The custom route.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> for further customization.</returns>
    public static IEndpointConventionBuilder MapShawarma(this IEndpointRouteBuilder endpoints, RoutePattern pattern) =>
        endpoints.Map(pattern, static context => {
            var requestHandler = context.RequestServices.GetRequiredService<IShawarmaRequestHandler>();

            return requestHandler.HandleRequest(context);
        });
}
