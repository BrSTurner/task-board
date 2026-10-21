using TaskBoard.Application.Auth;
using TaskBoard.Application.Common;
using TaskBoard.Application.Tests.Fakes;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tests.Auth;

public sealed class LoginUserTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly FakePasswordService _passwords = new();
    private readonly LoginUser _sut;
    private readonly User _alice;

    public LoginUserTests()
    {
        _alice = User.Create("Alice", "alice@example.test", "America/Sao_Paulo", _passwords.Hash("DemoPass123!"));
        _users.Users.Add(_alice);
        _sut = new LoginUser(_users, _passwords, new FakeTokenService());
    }

    [Fact]
    public async Task Valid_credentials_return_token_expiry_and_user()
    {
        var result = await _sut.ExecuteAsync(new LoginUserRequest { Email = "  ALICE@example.test ", Password = "DemoPass123!" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"token-for-{_alice.Id}", result.Value.AccessToken);
        Assert.Equal(FakeTokenService.ExpiresAt, result.Value.ExpiresAt);
        Assert.Equal(_alice.Id, result.Value.User.Id);
        Assert.Equal("America/Sao_Paulo", result.Value.User.TimeZone);
    }

    [Theory]
    [InlineData("alice@example.test", "WrongPass123!")]
    [InlineData("nobody@example.test", "DemoPass123!")]
    public async Task Wrong_password_and_unknown_email_fail_identically(string email, string password)
    {
        var result = await _sut.ExecuteAsync(new LoginUserRequest { Email = email, Password = password }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        Assert.Equal("Invalid email or password.", result.Error.Message);
    }
}
