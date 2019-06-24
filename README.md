# Shawarma.AspNetCore



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
