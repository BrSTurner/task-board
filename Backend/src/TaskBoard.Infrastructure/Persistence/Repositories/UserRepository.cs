using Microsoft.EntityFrameworkCore;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(TaskBoardDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> TryAddAsync(User user, CancellationToken cancellationToken)
    {
        var entry = db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (SqliteErrors.IsUniqueConstraintViolation(ex))
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }
}
