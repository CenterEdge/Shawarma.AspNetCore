using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shawarma.AspNetCore.Internal;

/// <summary>
/// Default implementation of <see cref="IShawarmaRequestHandler"/>.
/// </summary>
internal class ShawarmaRequestHandler : IShawarmaRequestHandler
{
    private static byte[]? _badRequest;
    private static byte[] BadRequest => _badRequest ??=
        new UTF8Encoding(false).GetBytes("Bad Request");

    private static readonly MediaTypeHeaderValue JsonMediaType = new(MediaTypeNames.Application.Json)
    {
        CharSet = Encoding.UTF8.WebName
    };

    private static readonly MediaTypeHeaderValue PlainTextMediaType = new(MediaTypeNames.Text.Plain)
    {
        CharSet = Encoding.UTF8.WebName
    };

    private readonly IApplicationStateProvider _stateProvider;
    private readonly ILogger<ShawarmaRequestHandler> _logger;

    public ShawarmaRequestHandler(
        IApplicationStateProvider stateProvider,
        ILogger<ShawarmaRequestHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(stateProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _stateProvider = stateProvider;
        _logger = logger;
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
                contentType.MediaType != JsonMediaType.MediaType)
            {
                httpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                return;
            }

            ApplicationState? state;
            var charset = contentType.CharSet;
            if (charset != null && charset != JsonMediaType.CharSet)
            {
                // Support transcoding if the request is not UTF-8 encoded

                Encoding encoding;

                // Remove at most a single set of quotes.
                if (charset.Length > 2 && charset[0] == '\"' && charset[^1] == '\"')
                {
                    encoding = Encoding.GetEncoding(charset[1..^1]);
                }
                else
                {
                    encoding = Encoding.GetEncoding(charset);
                }

                await using var transcodingStream =
                    Encoding.CreateTranscodingStream(request.Body, encoding, Encoding.UTF8, leaveOpen: true);

                state = await JsonSerializer.DeserializeAsync(transcodingStream,
                    ShawarmaSerialization.ApplicationStateTypeInfo, httpContext.RequestAborted);
            }
            else
            {
                state = await JsonSerializer.DeserializeAsync(request.Body,
                    ShawarmaSerialization.ApplicationStateTypeInfo, httpContext.RequestAborted);
            }

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

    private static Task ReturnState(HttpResponse response, ApplicationState state)
    {
        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = JsonMediaType.ToString();

        return JsonSerializer.SerializeAsync(response.Body, state, ShawarmaSerialization.ApplicationStateTypeInfo, response.HttpContext.RequestAborted);
    }

    private static ValueTask<FlushResult> ReturnBadRequest(HttpResponse response)
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
        response.ContentType = PlainTextMediaType.ToString();

        return response.BodyWriter.WriteAsync(BadRequest, response.HttpContext.RequestAborted);
    }
}
