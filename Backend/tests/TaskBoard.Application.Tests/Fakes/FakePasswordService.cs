using TaskBoard.Application.Abstractions;

namespace TaskBoard.Application.Tests.Fakes;

internal sealed class FakePasswordService : IPasswordService
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string passwordHash, string password) => passwordHash == Hash(password);
}
