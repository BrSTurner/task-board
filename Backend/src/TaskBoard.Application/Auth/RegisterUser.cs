using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Auth;

/// <summary>Registers a new account. Expects a request already validated by <see cref="RegisterUserRequestValidator"/>.</summary>
public sealed class RegisterUser(IUserRepository users, IPasswordService passwords)
{
    private static readonly Error EmailTaken = Error.Conflict("Auth.EmailTaken", "An account with this email already exists.");

    public async Task<Result<UserDto>> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(request.Email!);
        if (await users.GetByEmailAsync(email, cancellationToken) is not null)
        {
            return EmailTaken;
        }

        var user = User.Create(request.Name!, email, request.TimeZone!, passwords.Hash(request.Password!));

        // The pre-check above is only a fast path; the unique index is what guarantees uniqueness
        // when two registrations race.
        if (!await users.TryAddAsync(user, cancellationToken))
        {
            return EmailTaken;
        }

        return UserDto.From(user);
    }
}
