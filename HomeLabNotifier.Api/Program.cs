using HomeLabNotifier.Api;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();
