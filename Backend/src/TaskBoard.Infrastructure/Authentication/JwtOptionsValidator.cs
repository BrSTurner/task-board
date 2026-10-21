using System.Text;
using Microsoft.Extensions.Options;

namespace TaskBoard.Infrastructure.Authentication;

/// <summary>Fails application start-up on missing/weak JWT configuration instead of failing on the first login.</summary>
internal sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    private const int MinimumKeyBytes = 32; // HS256 requires a key of at least 256 bits.

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Jwt:Audience is required.");
        }

        if (Encoding.UTF8.GetByteCount(options.SigningKey ?? string.Empty) < MinimumKeyBytes)
        {
            failures.Add($"Jwt:SigningKey must be at least {MinimumKeyBytes} bytes. Configure it with user-secrets or an environment variable.");
        }

        if (options.ExpiryMinutes is < 1 or > 1440)
        {
            failures.Add("Jwt:ExpiryMinutes must be between 1 and 1440.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
