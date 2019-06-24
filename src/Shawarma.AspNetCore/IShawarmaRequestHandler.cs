using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Shawarma.AspNetCore
{
    /// <summary>
    /// Handle an HTTP request intercepted by <see cref="ShawarmaMiddleware"/>.
    /// </summary>
    public interface IShawarmaRequestHandler
    {
        /// <summary>
        /// Handles the request, providing a response via <see cref="HttpContext.Response"/>.
        /// </summary>
        /// <param name="httpContext">The request <see cref="HttpContext"/>.</param>
        /// <param name="routeValues">Route values parsed from the route template.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task HandleRequest(HttpContext httpContext, RouteValueDictionary routeValues);
    }
}
