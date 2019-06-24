using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;

namespace Shawarma.AspNetCore
{
    /// <summary>
    /// Middleware component to intercept Shawarma-related requests.
    /// </summary>
    public class ShawarmaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShawarmaRequestHandler _requestHandler;
        private readonly ShawarmaOptions _options;
        private readonly TemplateMatcher _requestMatcher;

        public ShawarmaMiddleware(
            RequestDelegate next,
            IShawarmaRequestHandler requestHandler,
            IOptions<ShawarmaOptions> options)
            : this(next, requestHandler, options.Value)
        {
        }

        public ShawarmaMiddleware(
            RequestDelegate next,
            IShawarmaRequestHandler requestHandler,
            ShawarmaOptions options)
        {
            _next = next;
            _requestHandler = requestHandler;
            _options = options;
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(options.RouteTemplate),
                options.RouteDefaults ?? new RouteValueDictionary());
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!RequestingApplicationState(httpContext.Request, out RouteValueDictionary routeValues))
            {
                // Ignore and pass to next middleware
                await _next(httpContext);
                return;
            }

            // Forward to the request handler
            await _requestHandler.HandleRequest(httpContext, routeValues);
        }

        private bool RequestingApplicationState(HttpRequest request, out RouteValueDictionary routeValues)
        {
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsPost(request.Method))
            {
                routeValues = null;
                return false;
            }

            routeValues = new RouteValueDictionary();
            return _requestMatcher.TryMatch(request.Path, routeValues);
        }
    }
}
