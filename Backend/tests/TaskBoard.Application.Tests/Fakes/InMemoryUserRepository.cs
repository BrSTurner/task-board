using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tests.Fakes;

/// <summary>Hand-written fake; relational behaviour (unique index) is covered by Infrastructure tests.</summary>
internal sealed class InMemoryUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    /// <summary>Simulates a concurrent registration winning the unique-index race.</summary>
    public bool SimulateUniqueViolation { get; set; }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(u => u.Email == normalizedEmail));

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Any(u => u.Id == id));

    public Task<bool> TryAddAsync(User user, CancellationToken cancellationToken)
    {
        if (SimulateUniqueViolation || Users.Any(u => u.Email == user.Email))
        {
            return Task.FromResult(false);
        }

        Users.Add(user);
        return Task.FromResult(true);
    }
}
