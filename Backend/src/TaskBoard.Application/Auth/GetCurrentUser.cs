using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Auth;

public sealed class GetCurrentUser(IUserRepository users, ICurrentUser currentUser)
{
    public Task<Result<UserDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _ = (users, currentUser); // TDD stub
        return Task.FromResult<Result<UserDto>>(Error.Conflict("Stub", "TDD stub"));
    }
}
