using Microsoft.AspNetCore.Diagnostics;

namespace TaskBoard.Api.Http;

/// <summary>Writes <see cref="ApiResult"/> error envelopes for responses produced outside endpoint handlers.</summary>
public static class ErrorHandling
{
    public static IServiceCollection AddTaskBoardErrorHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        // Surface model-binding failures (malformed JSON, invalid enum values…) as exceptions in every
        // environment so GlobalExceptionHandler can wrap them; the default only throws in Development.
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        return services;
    }

    public static WebApplication UseTaskBoardErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler(_ => { });
        // Any 4xx/5xx without a body (unknown route, JWT challenge, 405, 415…) gets the envelope too.
        app.UseStatusCodePages(context =>
            WriteAsync(context.HttpContext, context.HttpContext.Response.StatusCode, ForStatus(context.HttpContext.Response.StatusCode)));
        return app;
    }

    public static Task WriteAsync(HttpContext httpContext, int statusCode, ApiError error) =>
        Results.Json(ApiResult.Failure(error), statusCode: statusCode).ExecuteAsync(httpContext);

    public static ApiError ForStatus(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => new() { Code = "BadRequest", Message = "The request is invalid." },
        StatusCodes.Status401Unauthorized => new() { Code = "Unauthorized", Message = "Authentication is required. Please sign in." },
        StatusCodes.Status403Forbidden => new() { Code = "Forbidden", Message = "You are not allowed to perform this action." },
        StatusCodes.Status404NotFound => new() { Code = "NotFound", Message = "The requested resource was not found." },
        StatusCodes.Status405MethodNotAllowed => new() { Code = "MethodNotAllowed", Message = "The HTTP method is not allowed for this resource." },
        StatusCodes.Status413PayloadTooLarge => new() { Code = "PayloadTooLarge", Message = "The request body is too large." },
        StatusCodes.Status415UnsupportedMediaType => new() { Code = "UnsupportedMediaType", Message = "The request content type must be application/json." },
        >= 500 => new() { Code = "ServerError", Message = "An unexpected error occurred. Please try again later." },
        _ => new() { Code = "Error", Message = "The request could not be processed." },
    };
}

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, error) = exception switch
        {
            BadHttpRequestException bad => (bad.StatusCode, bad.StatusCode == StatusCodes.Status400BadRequest
                ? new ApiError { Code = "BadRequest", Message = "The request body is malformed or contains invalid values." }
                : ErrorHandling.ForStatus(bad.StatusCode)),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ErrorHandling.ForStatus(StatusCodes.Status401Unauthorized)),
            _ => (StatusCodes.Status500InternalServerError, ErrorHandling.ForStatus(StatusCodes.Status500InternalServerError)),
        };

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Rejected request {Method} {Path}: {Reason}", httpContext.Request.Method, httpContext.Request.Path, exception.Message);
        }

        await ErrorHandling.WriteAsync(httpContext, statusCode, error);
        return true;
    }
}
