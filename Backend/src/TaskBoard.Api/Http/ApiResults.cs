using TaskBoard.Application.Common;

namespace TaskBoard.Api.Http;

/// <summary>Maps application <see cref="Result"/>s to HTTP responses carrying the <see cref="ApiResult"/> envelope.</summary>
internal static class ApiResults
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : result.Error!.ToHttpResult();

    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess) =>
        result.IsSuccess ? onSuccess() : result.Error!.ToHttpResult();

    public static IResult ToHttpResult(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Error(statusCode, new ApiError
        {
            Code = error.Code,
            Message = error.Message,
            ValidationErrors = error.ValidationErrors,
        });
    }

    public static IResult Ok<T>(T data) => TypedResults.Ok(ApiResult.Success(data));

    public static IResult ValidationProblem(IReadOnlyDictionary<string, string[]> errors) =>
        Error(StatusCodes.Status400BadRequest, new ApiError
        {
            Code = "Validation",
            Message = "One or more validation errors occurred.",
            ValidationErrors = errors,
        });

    public static IResult Error(int statusCode, ApiError error) =>
        TypedResults.Json(ApiResult.Failure(error), statusCode: statusCode);

    /// <summary>Declares envelope error responses in OpenAPI (handlers return <see cref="IResult"/>, so metadata is explicit).</summary>
    public static TBuilder ProducesApiError<TBuilder>(this TBuilder builder, params int[] statusCodes)
        where TBuilder : IEndpointConventionBuilder
    {
        foreach (var statusCode in statusCodes)
        {
            builder.WithMetadata(new ProducesResponseTypeMetadata(statusCode, typeof(ApiResult), ["application/json"]));
        }

        return builder;
    }
}
