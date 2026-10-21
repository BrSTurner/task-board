using TaskBoard.Application.Abstractions;

namespace TaskBoard.Application.Tests.Fakes;

internal sealed class FakeCurrentUser(Guid userId, string timeZoneId = "America/Sao_Paulo") : ICurrentUser
{
    public Guid UserId { get; } = userId;

    public string TimeZoneId { get; } = timeZoneId;
}
