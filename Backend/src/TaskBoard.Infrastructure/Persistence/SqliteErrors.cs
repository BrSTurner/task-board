using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TaskBoard.Infrastructure.Persistence;

internal static class SqliteErrors
{
    private const int SqliteConstraint = 19;           // SQLITE_CONSTRAINT
    private const int SqliteConstraintUnique = 2067;   // SQLITE_CONSTRAINT_UNIQUE

    public static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException
        {
            SqliteErrorCode: SqliteConstraint,
            SqliteExtendedErrorCode: SqliteConstraintUnique,
        };
}
