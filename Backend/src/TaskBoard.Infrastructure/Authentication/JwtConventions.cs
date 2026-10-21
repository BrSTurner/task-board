using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskBoard.Infrastructure.Authentication;

/// <summary>Shared by token issuing (Infrastructure) and token validation (API composition root).</summary>
public static class JwtConventions
{
    /// <summary>OIDC standard claim for the user's IANA timezone.</summary>
    public const string TimeZoneClaim = "zoneinfo";

    public static SymmetricSecurityKey CreateSigningKey(string signingKey) => new(Encoding.UTF8.GetBytes(signingKey));
}
