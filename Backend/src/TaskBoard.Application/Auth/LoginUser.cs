using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Auth;

public sealed class LoginUser(IUserRepository users, IPasswordService passwords, ITokenService tokens)
{
    public Task<Result<LoginResponse>> ExecuteAsync(LoginUserRequest request, CancellationToken cancellationToken)
    {
        _ = (users, passwords, tokens); // TDD stub
        return Task.FromResult<Result<LoginResponse>>(Error.Conflict("Stub", "TDD stub"));
    }
}
