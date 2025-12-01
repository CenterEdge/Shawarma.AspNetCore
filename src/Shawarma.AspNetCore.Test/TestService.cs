using Shawarma.AspNetCore.Hosting;

namespace Shawarma.AspNetCore.Test;

public class TestService(ILogger<TestService> logger) : GenericShawarmaService(logger)
{
    protected override Task StartInternalAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override Task StopInternalAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
