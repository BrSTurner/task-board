using Microsoft.AspNetCore.Identity;
using TaskBoard.Application.Abstractions;

namespace TaskBoard.Infrastructure.Authentication;

/// <summary>Adapter over ASP.NET Core Identity's maintained <see cref="PasswordHasher{TUser}"/> (PBKDF2, salted).</summary>
internal sealed class PasswordService : IPasswordService
{
    // The default hasher ignores the user instance, so a marker type keeps the domain free of Identity types.
    private static readonly PasswordHasher<PasswordService> Hasher = new();

    public string Hash(string password) => Hasher.HashPassword(this, password);

    public bool Verify(string passwordHash, string password)
    {
        try
        {
            // SuccessRehashNeeded still means the password is correct.
            return Hasher.VerifyHashedPassword(this, passwordHash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
