using TaskBoard.Domain.Exceptions;

namespace TaskBoard.Domain;

internal static class Guard
{
    /// <summary>Returns the trimmed value, or throws when it is null/whitespace or longer than <paramref name="maxLength"/>.</summary>
    public static string RequiredText(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new DomainException($"{field} is required.");
        }

        return MaxLength(trimmed, field, maxLength);
    }

    public static string MaxLength(string value, string field, int maxLength) =>
        value.Length <= maxLength
            ? value
            : throw new DomainException($"{field} must be at most {maxLength} characters.");
}
