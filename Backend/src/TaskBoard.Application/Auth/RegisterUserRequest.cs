using FluentValidation;

namespace TaskBoard.Application.Auth;

public sealed class RegisterUserRequest
{
    public string? Name { get; init; }

    public string? Email { get; init; }

    /// <summary>IANA timezone id, e.g. <c>America/Sao_Paulo</c>.</summary>
    public string? TimeZone { get; init; }

    public string? Password { get; init; }
}

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    // TDD stub — rules are added in the GREEN step.
}
