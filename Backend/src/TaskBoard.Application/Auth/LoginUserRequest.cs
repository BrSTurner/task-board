using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace TaskBoard.Application.Auth;

public sealed class LoginUserRequest
{
    [Required]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    // TDD stub — rules are added in the GREEN step.
}

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    /// <summary>UTC instant after which the access token is rejected.</summary>
    public required DateTime ExpiresAt { get; init; }

    public required UserDto User { get; init; }
}
