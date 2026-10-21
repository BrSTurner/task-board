using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Authentication;

namespace TaskBoard.Infrastructure.Tests.Authentication;

public sealed class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "taskboard-tests",
        Audience = "taskboard-tests-client",
        SigningKey = "test-signing-key-that-is-at-least-32-bytes-long!",
        ExpiryMinutes = 30,
    };

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Token_is_signed_and_carries_identity_and_timezone_claims()
    {
        var user = User.Create("Alice", "alice@example.test", "America/Sao_Paulo", "hash");

        var token = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options), _time).CreateAccessToken(user);

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, new TokenValidationParameters
        {
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SigningKey)),
            // Validate against the fake clock, not the machine clock.
            LifetimeValidator = (_, _, _, _) => true,
        });

        Assert.True(validation.IsValid, validation.Exception?.Message);
        Assert.Equal(user.Id.ToString(), validation.Claims[JwtRegisteredClaimNames.Sub]);
        Assert.Equal("alice@example.test", validation.Claims[JwtRegisteredClaimNames.Email]);
        Assert.Equal("Alice", validation.Claims[JwtRegisteredClaimNames.Name]);
        Assert.Equal("America/Sao_Paulo", validation.Claims["zoneinfo"]);
    }

    [Fact]
    public void Expiry_is_now_plus_configured_minutes()
    {
        var user = User.Create("Alice", "alice@example.test", "UTC", "hash");

        var token = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options), _time).CreateAccessToken(user);

        Assert.Equal(new DateTime(2026, 10, 21, 12, 30, 0, DateTimeKind.Utc), token.ExpiresAtUtc);
        Assert.Equal(DateTimeKind.Utc, token.ExpiresAtUtc.Kind);
        Assert.Equal(token.ExpiresAtUtc, new JsonWebToken(token.Token).ValidTo);
    }

    [Theory]
    [InlineData("too-short")]
    [InlineData("")]
    public void Options_validation_rejects_signing_keys_shorter_than_32_bytes(string key)
    {
        var result = new JwtOptionsValidator().Validate(null, new JwtOptions { Issuer = "i", Audience = "a", SigningKey = key, ExpiryMinutes = 60 });

        Assert.True(result.Failed);
    }

    [Fact]
    public void Options_validation_accepts_complete_configuration()
    {
        Assert.True(new JwtOptionsValidator().Validate(null, Options).Succeeded);
    }
}
