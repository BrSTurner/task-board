using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Authentication;

internal sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private static readonly JsonWebTokenHandler Handler = new();

    public AccessToken CreateAccessToken(User user)
    {
        var jwt = options.Value;
        // JWT timestamps have second precision; truncate so ExpiresAtUtc matches the token's "exp" exactly.
        var now = DateTimeOffset.FromUnixTimeSeconds(timeProvider.GetUtcNow().ToUnixTimeSeconds()).UtcDateTime;
        var expires = now.AddMinutes(jwt.ExpiryMinutes);

        var token = Handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Name] = user.Name,
                [JwtConventions.TimeZoneClaim] = user.TimeZone,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            },
            SigningCredentials = new SigningCredentials(JwtConventions.CreateSigningKey(jwt.SigningKey), SecurityAlgorithms.HmacSha256),
        });

        return new AccessToken(token, expires);
    }
}
