using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Auth;

public sealed class RegisterUser(IUserRepository users, IPasswordService passwords)
{
    public Task<Result<UserDto>> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        _ = (users, passwords); // TDD stub
        return Task.FromResult<Result<UserDto>>(Error.Conflict("Stub", "TDD stub"));
    }
}
