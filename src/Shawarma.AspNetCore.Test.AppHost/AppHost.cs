using Shawarma.Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var app = builder.AddProject<Projects.Shawarma_AspNetCore_Test>("app")
    .WithHttpHealthCheck("/health", endpointName: "https")
    .WithShawarma(autoStart: true);

builder.Build().Run();
