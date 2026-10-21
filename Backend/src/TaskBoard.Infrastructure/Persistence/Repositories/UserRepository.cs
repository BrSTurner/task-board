using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(TaskBoardDbContext db) : IUserRepository
{
    // TDD stub — implemented in the GREEN step.
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<User?>(null);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) => Task.FromResult<User?>(null);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(false);

    public Task<bool> TryAddAsync(User user, CancellationToken cancellationToken)
    {
        _ = db;
        return Task.FromResult(true);
    }
}
