using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Shawarma.AspNetCore
{
    /// <summary>
    /// Provides the current <see cref="ApplicationState"/> with a <see cref="IChangeToken"/> to monitor for state changes.
    /// </summary>
    public interface IApplicationStateProvider
    {
        /// <summary>
        /// Get a change token that will trigger when the application state changes.
        /// </summary>
        /// <returns>The <see cref="IChangeToken"/>.</returns>
        IChangeToken GetChangeToken();

        /// <summary>
        /// Get the current application state.
        /// </summary>
        /// <returns>The <see cref="ApplicationState"/>.</returns>
        ValueTask<ApplicationState> GetApplicationState();

        /// <summary>
        /// Sets the current application state.
        /// </summary>
        /// <param name="state">The new <see cref="ApplicationState"/>.</param>
        /// <returns>The <see cref="ApplicationState"/>.</returns>
        Task SetApplicationState(ApplicationState state);
    }
}
