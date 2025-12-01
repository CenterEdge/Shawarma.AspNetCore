using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Shawarma.AspNetCore.Internal;

namespace Shawarma.AspNetCore;

/// <summary>
/// Middleware component to intercept Shawarma-related requests.
/// </summary>
[Obsolete(ShawarmaConstants.UseEndpointRoutingWarning)]
public class ShawarmaMiddleware(
    RequestDelegate next,
    IShawarmaRequestHandler requestHandler,
    ShawarmaOptions options)
{
    private readonly TemplateMatcher _requestMatcher = new(TemplateParser.Parse(options.RouteTemplate),
            options.RouteDefaults ?? []);

    public ShawarmaMiddleware(
        RequestDelegate next,
        IShawarmaRequestHandler requestHandler,
        IOptions<ShawarmaOptions> options)
        : this(next, requestHandler, options.Value)
    {
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (!RequestingApplicationState(httpContext.Request, out _))
        {
            // Ignore and pass to next middleware
            await next(httpContext);
            return;
        }

        // Forward to the request handler
        await requestHandler.HandleRequest(httpContext);
    }

    private bool RequestingApplicationState(HttpRequest request, out RouteValueDictionary routeValues)
    {
        routeValues = [];
        return _requestMatcher.TryMatch(request.Path, routeValues);
    }
}
