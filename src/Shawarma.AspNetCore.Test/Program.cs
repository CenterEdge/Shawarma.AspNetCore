using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shawarma.AspNetCore;
using Shawarma.AspNetCore.Hosting;
using Shawarma.AspNetCore.Test;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddShawarmaHosting()
    .AddShawarmaService<TestService>()
    .AddHealthChecks();

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation();
    })
    .WithMetrics(builder =>
    {
        builder
            .AddAspNetCoreInstrumentation();
    })
    .UseOtlpExporter();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapShawarma();
app.MapHealthChecks("/health");
app.MapGet("/", () => "Hello World!");

await app.RunAsync();
