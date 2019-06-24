# Shawarma.AspNetCore

[![Build Status](https://travis-ci.org/CenterEdge/Shawarma.AspNetCore.svg?branch=master)](https://travis-ci.org/CenterEdge/Shawarma.AspNetCore)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Shawarma.AspNetCore.svg)](https://www.nuget.org/packages/Shawarma.AspNetCore)

ASP.NET Core middleware and service hosting library designed to interact with Shawarma. This allows
background services, implemented similarly to [IHostedService](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2&tabs=visual-studio),
to be started and stopped based on whether the application instance is live on a Kubernetes
load balancer. This assists with blue/green deployments to Kubernetes, and ensures that
old application instances stop background processing of things like message queues.

For more details about Shawarma, see <https://github.com/CenterEdge/shawarma>.

## Usage

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddShawarmaHosting()
        // Add any IShawarmaService instances to be managed
        .AddShawarmaService<TestService>();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Add before MVC or other handlers
    app.UseShawarma();

    app.Run(async (context) =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
}
```

```cs
public class TestService : GenericShawarmaService
{
    public TestService(ILogger<TestService> logger)
        : base(logger)
    {
    }

    protected override Task StartInternalAsync(CancellationToken cancellationToken)
    {
        // Start doing work here
        return Task.CompletedTask;
    }

    protected override Task StopInternalAsync(CancellationToken cancellationToken)
    {
        // Stop doing work here
        return Task.CompletedTask;
    }
}
```
