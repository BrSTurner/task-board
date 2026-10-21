using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Infrastructure.Persistence;

namespace TaskBoard.Infrastructure.Tests;

/// <summary>
/// An isolated SQLite <b>in-memory</b> database (real relational engine, not the EF InMemory provider).
/// The connection stays open for the lifetime of the test because the database disappears when it closes.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        // EnsureCreated is acceptable for isolated test databases only; the app uses migrations.
        context.Database.EnsureCreated();
    }

    /// <summary>Each call returns a fresh context (empty change tracker) over the same database.</summary>
    public TaskBoardDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TaskBoardDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
