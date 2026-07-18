using HomeLabCore.Api.Endpoints;
using HomeLabCore.Application.Interfaces;

namespace HomeLabCore.Api.Setup;

public static class WebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication application)
    {
        application
            .MapGroup("/api")
            .MapHealthEndpoints();

        return application;
    }

    public static async Task InitializeApplication(this WebApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        var cancellationToken = lifetime.ApplicationStopping;

        using var scope = app.Services.CreateScope();

        var initializers = scope.ServiceProvider.GetServices<IApplicationInitializer>();

        foreach (var initializer in initializers.OrderBy(x => x.Order))
        {
            await initializer.InitializeAsync(cancellationToken);
        }
    }
}
