using System;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace Shawarma.AspNetCore.Internal
{
    /// <summary>
    /// Change token for notifying regarding changes to application state.
    /// </summary>
    internal class ApplicationStateChangeToken : IChangeToken
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <inheritdoc/>
        public bool ActiveChangeCallbacks => true;

        /// <inheritdoc/>
        public bool HasChanged => _cts.IsCancellationRequested;

        /// <inheritdoc/>
        public IDisposable RegisterChangeCallback(Action<object?> callback, object state) => _cts.Token.Register(callback, state);

        /// <summary>
        /// Triggers the token once a state change occurs.
        /// </summary>
        public void OnStateChange() => _cts.Cancel();
    }
}
