using Microsoft.AspNetCore.Http;

namespace Shawarma.AspNetCore;

/// <summary>
/// Handle an HTTP request intercepted by <see cref="ShawarmaMiddleware"/>.
/// </summary>
public interface IShawarmaRequestHandler
{
    /// <summary>
    /// Handles the request, providing a response via <see cref="HttpContext.Response"/>.
    /// </summary>
    /// <param name="httpContext">The request <see cref="HttpContext"/>.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task HandleRequest(HttpContext httpContext);
}
