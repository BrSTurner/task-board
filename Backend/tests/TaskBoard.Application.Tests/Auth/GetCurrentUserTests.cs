using TaskBoard.Application.Auth;
using TaskBoard.Application.Common;
using TaskBoard.Application.Tests.Fakes;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tests.Auth;

public sealed class GetCurrentUserTests
{
    private readonly InMemoryUserRepository _users = new();

    [Fact]
    public async Task Returns_the_authenticated_user()
    {
        var alice = User.Create("Alice", "alice@example.test", "America/Sao_Paulo", "hash");
        _users.Users.Add(alice);

        var result = await new GetCurrentUser(_users, new FakeCurrentUser(alice.Id)).ExecuteAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("alice@example.test", result.Value.Email);
    }

    [Fact]
    public async Task Valid_token_for_a_user_that_no_longer_exists_is_unauthorized()
    {
        var result = await new GetCurrentUser(_users, new FakeCurrentUser(Guid.NewGuid())).ExecuteAsync(CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }
}
