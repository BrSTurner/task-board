using Microsoft.Extensions.Options;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Authentication;

internal sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    public AccessToken CreateAccessToken(User user)
    {
        _ = (options, timeProvider); // TDD stub
        return new AccessToken(string.Empty, DateTime.MinValue);
    }
}
