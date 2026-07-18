using HomeLabCore.Api.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapEndpoints();

await app.InitializeApplication();

await app.RunAsync();
