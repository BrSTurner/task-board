using TaskBoard.Application.Auth;
using TaskBoard.Application.Common;
using TaskBoard.Application.Tests.Fakes;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tests.Auth;

public sealed class RegisterUserTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly RegisterUser _sut;

    public RegisterUserTests() => _sut = new RegisterUser(_users, new FakePasswordService());

    private static RegisterUserRequest Request(string email = "Alice@Example.test") => new()
    {
        Name = " Alice Martins ",
        Email = email,
        TimeZone = "America/Sao_Paulo",
        Password = "DemoPass123!",
    };

    [Fact]
    public async Task Creates_user_with_hashed_password_and_returns_safe_dto()
    {
        var result = await _sut.ExecuteAsync(Request(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(_users.Users);
        Assert.Equal("hashed:DemoPass123!", stored.PasswordHash);
        Assert.Equal(stored.Id, result.Value.Id);
        Assert.Equal("Alice Martins", result.Value.Name);
        Assert.Equal("alice@example.test", result.Value.Email);
        Assert.Equal("America/Sao_Paulo", result.Value.TimeZone);
    }

    [Theory]
    [InlineData("alice@example.test")]
    [InlineData("  ALICE@example.TEST ")]
    public async Task Duplicate_email_is_a_conflict_regardless_of_case_and_whitespace(string duplicate)
    {
        _users.Users.Add(User.Create("Existing", "alice@example.test", "UTC", "hash"));

        var result = await _sut.ExecuteAsync(Request(duplicate), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Single(_users.Users);
    }

    [Fact]
    public async Task Unique_index_violation_during_insert_is_a_conflict()
    {
        _users.SimulateUniqueViolation = true;

        var result = await _sut.ExecuteAsync(Request(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("Auth.EmailTaken", result.Error.Code);
    }
}
