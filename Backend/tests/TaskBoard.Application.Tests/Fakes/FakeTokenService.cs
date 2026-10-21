using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tests.Fakes;

internal sealed class FakeTokenService : ITokenService
{
    public static readonly DateTime ExpiresAt = new(2026, 10, 21, 19, 0, 0, DateTimeKind.Utc);

    public AccessToken CreateAccessToken(User user) => new($"token-for-{user.Id}", ExpiresAt);
}
