using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Shawarma.AspNetCore.Internal;

/// <summary>
/// Default implementation of <see cref="IApplicationStateProvider"/>.
/// </summary>
internal class ApplicationStateProvider : IApplicationStateProvider
{
    private readonly ILogger<ApplicationStateProvider> _logger;
    private volatile ApplicationState _state;
    private ApplicationStateChangeToken _changeToken = new();

    public ApplicationStateProvider(ILogger<ApplicationStateProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _state = new ApplicationState
        {
            Status = ApplicationStatus.Inactive
        };
    }

    /// <inheritdoc />
    public IChangeToken GetChangeToken() => _changeToken;

    /// <inheritdoc />
    public ValueTask<ApplicationState> GetApplicationStateAsync()
    {
        return new ValueTask<ApplicationState>(_state);
    }

    /// <inheritdoc />
    public Task SetApplicationStateAsync(ApplicationState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _logger.LogDebug("Received application state change: {status}", state.Status);
        _state = state;

        var changeToken = Interlocked.Exchange(ref _changeToken, new ApplicationStateChangeToken());
        changeToken.OnStateChange();

        return Task.CompletedTask;
    }
}
