using System;
using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shawarma.AspNetCore.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IShawarmaRequestHandler"/>.
    /// </summary>
    internal class ShawarmaRequestHandler : IShawarmaRequestHandler
    {
        private static byte[]? _badRequest;
        private static byte[] BadRequest => _badRequest ??=
            new UTF8Encoding(false).GetBytes("Bad Request");

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
        public Task HandleRequest(HttpContext httpContext)
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
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
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

                var state = await JsonSerializer.DeserializeAsync(request.Body,
                    ShawarmaSerializerContext.Primary.ApplicationState, httpContext.RequestAborted);

                if (state is not null)
                {
                    await _stateProvider.SetApplicationStateAsync(state);

                    // Reserialize the state onto the response
                    await ReturnState(httpContext.Response, state);
                }
                else
                {
                    await ReturnBadRequest(httpContext.Response);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing request body.");

                await ReturnBadRequest(httpContext.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Shawarma POST");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        private Task ReturnState(HttpResponse response, ApplicationState state)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "application/json; charset=utf-8";

            return JsonSerializer.SerializeAsync(response.Body, state, ShawarmaSerializerContext.Primary.ApplicationState, response.HttpContext.RequestAborted);
        }

        private ValueTask<FlushResult> ReturnBadRequest(HttpResponse response)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.ContentType = "text/plain; charset=utf-8";

            return response.BodyWriter.WriteAsync(BadRequest, response.HttpContext.RequestAborted);
        }
    }
}
