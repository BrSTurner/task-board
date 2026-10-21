using System.ComponentModel.DataAnnotations;
using FluentValidation;
using TaskBoard.Application.Common;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Auth;

/// <remarks>
/// <c>[Required]</c> only documents the OpenAPI contract (minimal API DataAnnotations validation is not enabled);
/// FluentValidation enforces the rules and produces the user-facing messages. Missing JSON properties bind to
/// empty strings so they fail with "… is required" instead of a generic binding error.
/// </remarks>
public sealed class RegisterUserRequest
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Email { get; init; } = string.Empty;

    /// <summary>IANA timezone id, e.g. <c>America/Sao_Paulo</c>.</summary>
    [Required]
    public string TimeZone { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 100;

    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Name).RequiredText("Name", User.NameMaxLength);

        RuleFor(x => x.Email).RequiredText("Email", User.EmailMaxLength)
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.TimeZone).Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Time zone is required")
            .Must(value => TimeZones.TryFindIana(value, out _)).WithMessage("Time zone must be a valid IANA time zone");

        // Same policy is enforced by the Angular register form.
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required")
            .Length(PasswordMinLength, PasswordMaxLength).WithMessage($"Password must be between {PasswordMinLength} and {PasswordMaxLength} characters")
            .Matches("[A-Z]").WithMessage("Password must contain an upper-case letter")
            .Matches("[a-z]").WithMessage("Password must contain a lower-case letter")
            .Matches("[0-9]").WithMessage("Password must contain a digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a symbol");
    }
}
