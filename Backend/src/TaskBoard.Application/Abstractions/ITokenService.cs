using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Abstractions;

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
}
