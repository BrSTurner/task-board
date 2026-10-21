using TaskBoard.Application.Auth;

namespace TaskBoard.Application.Tests.Auth;

public sealed class RegisterUserRequestValidatorTests
{
    private readonly RegisterUserRequestValidator _validator = new();

    private static RegisterUserRequest Request(
        string? name = "Alice Martins",
        string? email = "alice@example.test",
        string? timeZone = "America/Sao_Paulo",
        string? password = "DemoPass123!") =>
        new() { Name = name!, Email = email!, TimeZone = timeZone!, Password = password! };

    [Fact]
    public void Valid_request_passes()
    {
        Assert.True(_validator.Validate(Request()).IsValid);
    }

    public static TheoryData<RegisterUserRequest, string, string> MissingFields => new()
    {
        { Request(name: "   "), nameof(RegisterUserRequest.Name), "Name is required" },
        { Request(email: null), nameof(RegisterUserRequest.Email), "Email is required" },
        { Request(timeZone: ""), nameof(RegisterUserRequest.TimeZone), "Time zone is required" },
        { Request(password: null), nameof(RegisterUserRequest.Password), "Password is required" },
    };

    [Theory]
    [MemberData(nameof(MissingFields))]
    public void Missing_required_field_reports_a_single_required_message(RegisterUserRequest request, string field, string message)
    {
        var result = _validator.Validate(request);

        var error = Assert.Single(result.Errors, e => e.PropertyName == field);
        Assert.Equal(message, error.ErrorMessage);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    public void Invalid_email_is_rejected(string email)
    {
        var result = _validator.Validate(Request(email: email));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserRequest.Email));
    }

    [Fact]
    public void Too_long_name_is_rejected()
    {
        var result = _validator.Validate(Request(name: new string('a', 101)));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserRequest.Name));
    }

    [Fact]
    public void Non_iana_time_zone_is_rejected()
    {
        var result = _validator.Validate(Request(timeZone: "E. South America Standard Time"));

        var error = Assert.Single(result.Errors, e => e.PropertyName == nameof(RegisterUserRequest.TimeZone));
        Assert.Equal("Time zone must be a valid IANA time zone", error.ErrorMessage);
    }

    [Theory]
    [InlineData("Short1!")]        // fewer than 8 characters
    [InlineData("alllowercase1!")] // no upper-case letter
    [InlineData("ALLUPPERCASE1!")] // no lower-case letter
    [InlineData("NoDigitsHere!")]  // no digit
    [InlineData("NoSymbols123")]   // no symbol
    public void Weak_password_is_rejected(string password)
    {
        var result = _validator.Validate(Request(password: password));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserRequest.Password));
    }

    [Fact]
    public void Password_longer_than_100_characters_is_rejected()
    {
        var result = _validator.Validate(Request(password: "Aa1!" + new string('x', 97)));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserRequest.Password));
    }
}
