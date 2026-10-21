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
    public LoginUserRequestValidator()
    {
        // Presence only: applying the registration policy here would reveal password rules for existing accounts.
        RuleFor(x => x.Email).Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Email is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
    }
}

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    /// <summary>UTC instant after which the access token is rejected.</summary>
    public required DateTime ExpiresAt { get; init; }

    public required UserDto User { get; init; }
}
