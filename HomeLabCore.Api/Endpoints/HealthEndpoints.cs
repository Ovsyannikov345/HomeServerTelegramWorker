namespace HomeLabCore.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/health")
            .WithTags("Health");

        group.MapGet("/ping", () => Results.Ok(new { status = "Healthy", Message = "pong", Timestamp = DateTimeOffset.UtcNow }))
            .WithName("PingHealthCheck");
    }
}
