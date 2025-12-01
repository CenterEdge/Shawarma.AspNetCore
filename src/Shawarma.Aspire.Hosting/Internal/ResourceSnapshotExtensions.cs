using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting.ApplicationModel;

namespace Shawarma.Aspire.Hosting.Internal;

internal static class ResourceSnapshotExtensions
{
    private const string PropertyKey = "ShawarmaState";

    extension(CustomResourceSnapshot snapshot)
    {
        public bool TryGetState([NotNullWhen(true)] out ApplicationState? state)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            state = snapshot.Properties.FirstOrDefault(p => p.Name == PropertyKey)?.Value as ApplicationState;

            return state is not null;
        }

        public CustomResourceSnapshot SetState(ApplicationState? state)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            var newProperties = snapshot.Properties
                .RemoveAll(p => p.Name == PropertyKey);

            if (state is not null)
            {
                newProperties = newProperties.Add(new ResourcePropertySnapshot(PropertyKey, state));
            }

            if (snapshot.Properties.Equals(newProperties))
            {
                // No change
                return snapshot;
            }

            return snapshot with
            {
                Properties = newProperties
            };
        }
    }
}
