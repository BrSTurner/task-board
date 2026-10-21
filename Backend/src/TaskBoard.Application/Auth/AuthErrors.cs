using TaskBoard.Application.Common;

namespace TaskBoard.Application.Auth;

public static class AuthErrors
{
    public static readonly Error SessionUserNotFound =
        Error.Unauthorized("Auth.SessionUserNotFound", "Your session is no longer valid. Please sign in again.");
}
