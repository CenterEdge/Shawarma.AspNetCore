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
// This is an example in Program.cs using the Minimal API approach
using Shawarma.AspNetCore;
using Shawarma.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddShawarmaHosting()
    .AddShawarmaService<TestService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapShawarma();
app.MapGet("/", () => "Hello World!");

await app.RunAsync();
```

```cs
// Use this approach in Startup.cs if you are not using the Minimal API approach
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddRouting()
        .AddShawarmaHosting()
        // Add any IShawarmaService instances to be managed
        .AddShawarmaService<TestService>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapShawarma();

        endpoints.MapGet("", async context =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
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

## Older .NET Core Versions

Older versions of .NET Core are supported using middleware instead of endpoint routing.

```cs
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

## Developing

See [Developing Shawarma.AspNetCore](./Developing.md) for instructions for development.
