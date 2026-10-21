namespace TaskBoard.Api.Http;

/// <summary>
/// The single response envelope returned to the frontend for every response that has a body.
/// Error responses use this non-generic shape; success responses use <see cref="ApiResult{T}"/>.
/// </summary>
public class ApiResult
{
    public required bool IsSuccess { get; init; }

    public ApiError? Error { get; init; }

    public static ApiResult<T> Success<T>(T data) => new() { IsSuccess = true, Data = data };

    public static ApiResult Failure(ApiError error) => new() { IsSuccess = false, Error = error };
}

public sealed class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }
}

public sealed class ApiError
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    /// <summary>Field name (camelCase, matching the request JSON) → messages. Only present for validation failures.</summary>
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }
}
