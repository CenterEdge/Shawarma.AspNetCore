namespace Shawarma;

/// <summary>
/// An service which is stopped and started based on Shawarma's <see cref="ApplicationState"/>.
/// </summary>
public interface IShawarmaService
{
    /// <summary>
    /// Running state of the service.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Start or stop the service based on a new <see cref="ApplicationState"/>.
    /// </summary>
    /// <param name="state">The new <see cref="ApplicationState"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> triggered to give up on starting.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task UpdateStateAsync(ApplicationState state, CancellationToken cancellationToken);

    /// <summary>
    /// Stop the service.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> triggered to give up on stopping.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task StopAsync(CancellationToken cancellationToken);
}
