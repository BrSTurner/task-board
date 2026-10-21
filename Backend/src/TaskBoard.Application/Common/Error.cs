namespace TaskBoard.Application.Common;

public enum ErrorType
{
    Validation,
    Unauthorized,
    NotFound,
    Conflict,
}

/// <summary>An expected, user-facing failure. Unexpected failures are exceptions.</summary>
public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static Error Validation(string field, string message) =>
        new("Validation", "One or more validation errors occurred.", ErrorType.Validation,
            new Dictionary<string, string[]> { [field] = [message] });

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
}
