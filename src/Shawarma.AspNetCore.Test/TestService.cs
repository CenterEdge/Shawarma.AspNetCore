using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shawarma.AspNetCore.Hosting;

namespace Shawarma.AspNetCore.Test
{
    public class TestService : GenericShawarmaService
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public TestService(ILogger<TestService> logger)
            : base(logger)
        {
        }

        protected override Task StartInternalAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task StopInternalAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
