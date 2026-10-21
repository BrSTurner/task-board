using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <param name="normalizedEmail">An email already normalized with <see cref="User.NormalizeEmail"/>.</param>
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Persists a new user. Returns <c>false</c> when the email is already taken (unique index).</summary>
    Task<bool> TryAddAsync(User user, CancellationToken cancellationToken);
}
