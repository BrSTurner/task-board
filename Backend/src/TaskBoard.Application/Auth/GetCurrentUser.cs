using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Auth;

public sealed class GetCurrentUser(IUserRepository users, ICurrentUser currentUser)
{
    public async Task<Result<UserDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        // A token can outlive its user row (e.g. the local database was reset), so check it still exists.
        var user = await users.GetByIdAsync(currentUser.UserId, cancellationToken);
        return user is null ? AuthErrors.SessionUserNotFound : UserDto.From(user);
    }
}
