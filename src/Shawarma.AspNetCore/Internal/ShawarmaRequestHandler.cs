using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Shawarma.AspNetCore.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IShawarmaRequestHandler"/>.
    /// </summary>
    internal class ShawarmaRequestHandler : IShawarmaRequestHandler
    {
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private readonly IApplicationStateProvider _stateProvider;
        private readonly ILogger<ShawarmaRequestHandler> _logger;

        public ShawarmaRequestHandler(
            IApplicationStateProvider stateProvider,
            ILogger<ShawarmaRequestHandler> logger)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task HandleRequest(HttpContext httpContext, RouteValueDictionary routeValues)
        {
            if (HttpMethods.IsGet(httpContext.Request.Method))
            {
                return HandleGet(httpContext);
            }
            else if (HttpMethods.IsPost(httpContext.Request.Method))
            {
                return HandlePost(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return Task.CompletedTask;
            }
        }

        public async Task HandleGet(HttpContext httpContext)
        {
            try
            {
                _logger.LogDebug("Received Shawarma GET");

                var state = await _stateProvider.GetApplicationStateAsync();

                await ReturnState(httpContext.Response, state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Shawarma GET");
                httpContext.Response.StatusCode = 500;
            }
        }

        public async Task HandlePost(HttpContext httpContext)
        {
            try
            {
                _logger.LogDebug("Received Shawarma POST");

                var request = httpContext.Request;

                if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var contentType) ||
                    contentType.MediaType != "application/json" ||
                    (contentType.CharSet != null && contentType.CharSet != "utf-8"))
                {
                    httpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                }

                if (!request.Body.CanSeek)
                {
                    // JSON.Net does synchronous reads. In order to avoid blocking on the stream, we asynchronously
                    // read everything into a buffer, and then seek back to the beginning.
                    request.EnableBuffering();

                    await request.Body.DrainAsync(CancellationToken.None);
                    request.Body.Seek(0L, SeekOrigin.Begin);
                }

                ApplicationState state;
                // Don't use NoBom encoding on read as it's unnecessary, and the built-in encoder has some optimizations
                using (var streamReader = new StreamReader(request.Body, Encoding.UTF8))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        jsonReader.CloseInput = false;

                        var serializer = new JsonSerializer();
                        state = serializer.Deserialize<ApplicationState>(jsonReader);
                    }
                }

                await _stateProvider.SetApplicationStateAsync(state);

                // Reserialize the state onto the response
                await ReturnState(httpContext.Response, state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Shawarma POST");
                httpContext.Response.StatusCode = 500;
            }
        }

        private async Task ReturnState(HttpResponse response, ApplicationState state)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json; charset=utf-8";

            using (var streamWriter = new StreamWriter(response.Body, Utf8NoBom))
            {
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    var serializer = new JsonSerializer();
                    serializer.Serialize(jsonWriter, state);
                }

                await streamWriter.FlushAsync();
            }
        }
    }
}
