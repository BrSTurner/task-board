using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Persistence.Repositories;

namespace TaskBoard.Infrastructure.Tests.Repositories;

public sealed class UserRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    private static User NewUser(string email = "carol@example.test") =>
        User.Create("Carol", email, "Europe/London", "hash");

    [Fact]
    public async Task TryAddAsync_persists_user_that_can_be_found_by_email_and_id()
    {
        var user = NewUser();
        await using (var context = _database.CreateContext())
        {
            Assert.True(await new UserRepository(context).TryAddAsync(user, CancellationToken.None));
        }

        await using var readContext = _database.CreateContext();
        var repository = new UserRepository(readContext);
        var byEmail = await repository.GetByEmailAsync("carol@example.test", CancellationToken.None);
        var byId = await repository.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.NotNull(byEmail);
        Assert.Equal(user.Id, byEmail.Id);
        Assert.Equal("Carol", byEmail.Name);
        Assert.Equal("Europe/London", byEmail.TimeZone);
        Assert.Equal(user.Id, byId?.Id);
        Assert.True(await repository.ExistsAsync(user.Id, CancellationToken.None));
    }

    [Fact]
    public async Task TryAddAsync_returns_false_when_unique_email_index_is_violated()
    {
        await using (var first = _database.CreateContext())
        {
            Assert.True(await new UserRepository(first).TryAddAsync(NewUser(), CancellationToken.None));
        }

        // A second context simulates a concurrent registration that passed the application pre-check.
        await using var second = _database.CreateContext();
        var added = await new UserRepository(second).TryAddAsync(NewUser(" CAROL@example.test "), CancellationToken.None);

        Assert.False(added);
    }

    [Fact]
    public async Task Unknown_user_is_not_found()
    {
        await using var context = _database.CreateContext();
        var repository = new UserRepository(context);

        Assert.Null(await repository.GetByEmailAsync("nobody@example.test", CancellationToken.None));
        Assert.False(await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
