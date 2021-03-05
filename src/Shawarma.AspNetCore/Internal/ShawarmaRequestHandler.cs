using System;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

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

                var state = await JsonSerializer.DeserializeAsync<ApplicationState>(request.Body, SerializerOptions, httpContext.RequestAborted);

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

        private Task ReturnState(HttpResponse response, ApplicationState state)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json; charset=utf-8";

            return JsonSerializer.SerializeAsync(response.Body, state, SerializerOptions, response.HttpContext.RequestAborted);
        }
    }
}
