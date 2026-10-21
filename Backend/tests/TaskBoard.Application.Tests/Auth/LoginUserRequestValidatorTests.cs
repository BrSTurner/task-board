using TaskBoard.Application.Auth;

namespace TaskBoard.Application.Tests.Auth;

public sealed class LoginUserRequestValidatorTests
{
    private readonly LoginUserRequestValidator _validator = new();

    [Fact]
    public void Requires_email_and_password()
    {
        var result = _validator.Validate(new LoginUserRequest { Email = " ", Password = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginUserRequest.Email) && e.ErrorMessage == "Email is required");
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginUserRequest.Password) && e.ErrorMessage == "Password is required");
    }

    [Fact]
    public void Does_not_apply_the_registration_password_policy()
    {
        // Policy failures on login would leak which rule a stored password satisfies; just check presence.
        Assert.True(_validator.Validate(new LoginUserRequest { Email = "a@example.test", Password = "x" }).IsValid);
    }
}
