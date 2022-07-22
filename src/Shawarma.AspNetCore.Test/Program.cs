using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Shawarma.AspNetCore;
using Shawarma.AspNetCore.Hosting;
using Shawarma.AspNetCore.Test;

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
