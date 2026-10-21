using Microsoft.Extensions.Options;

namespace TaskBoard.Infrastructure.Authentication;

internal sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options) => ValidateOptionsResult.Success; // TDD stub
}
