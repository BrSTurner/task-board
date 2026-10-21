using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaskBoard.Application.Abstractions;
using TaskBoard.Infrastructure.Authentication;
using TaskBoard.Infrastructure.Persistence;
using TaskBoard.Infrastructure.Persistence.Repositories;

namespace TaskBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var connectionString = ResolveConnectionString(
            configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured."),
            contentRootPath);

        services.AddDbContext<TaskBoardDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordService, PasswordService>();

        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName)).ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }

    /// <summary>Applies pending EF Core migrations (schema + <c>HasData</c> seed). Never combined with EnsureCreated.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskBoardDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// A relative SQLite path is resolved against the API content root rather than the current working
    /// directory, so <c>dotnet run</c>, <c>dotnet ef</c> and the IDE all use the same database file.
    /// </summary>
    internal static string ResolveConnectionString(string connectionString, string contentRootPath)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (builder.DataSource != ":memory:" && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRootPath, builder.DataSource));
        }

        return builder.ToString();
    }
}
