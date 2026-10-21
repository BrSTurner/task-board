using TaskBoard.Api.Http;

namespace TaskBoard.Api.Endpoints;

public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public")
            .WithTags("Public")
            .AllowAnonymous();

        group.MapGet("/ping", (TimeProvider time) =>
                TypedResults.Ok(ApiResult.Success(new PingResponse
                {
                    Status = "ok",
                    ServerTimeUtc = time.GetUtcNow().UtcDateTime,
                })))
            .WithName("Ping")
            .WithSummary("Anonymous health check that demonstrates a non-authorized endpoint.");

        return app;
    }
}

public sealed class PingResponse
{
    public required string Status { get; init; }

    public required DateTime ServerTimeUtc { get; init; }
}
