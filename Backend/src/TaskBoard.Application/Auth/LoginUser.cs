using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Auth;

public sealed class LoginUser(IUserRepository users, IPasswordService passwords, ITokenService tokens)
{
    // Same error for unknown email and wrong password, so the response does not reveal which accounts exist.
    private static readonly Error InvalidCredentials = Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

    public async Task<Result<LoginResponse>> ExecuteAsync(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(User.NormalizeEmail(request.Email), cancellationToken);
        if (user is null)
        {
            // Spend comparable hashing time so response timing does not reveal unknown emails either.
            _ = passwords.Hash(request.Password);
            return InvalidCredentials;
        }

        if (!passwords.Verify(user.PasswordHash, request.Password))
        {
            return InvalidCredentials;
        }

        var token = tokens.CreateAccessToken(user);
        return new LoginResponse
        {
            AccessToken = token.Token,
            ExpiresAt = token.ExpiresAtUtc,
            User = UserDto.From(user),
        };
    }
}
