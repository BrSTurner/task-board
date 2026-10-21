using TaskBoard.Application.Abstractions;

namespace TaskBoard.Infrastructure.Authentication;

internal sealed class PasswordService : IPasswordService
{
    // TDD stub — implemented in the GREEN step.
    public string Hash(string password) => password;

    public bool Verify(string passwordHash, string password) => true;
}
