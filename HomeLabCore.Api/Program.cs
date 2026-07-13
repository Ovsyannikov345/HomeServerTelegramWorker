using HomeLabCore.Api;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app
    .MapEndpoints()
    .InitializeApplication();

await app.RunAsync();
