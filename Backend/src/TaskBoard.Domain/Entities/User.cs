namespace TaskBoard.Domain.Entities;

public sealed class User
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 254;
    public const int TimeZoneMaxLength = 100;

    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string TimeZone { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    // TDD stub — implemented in the GREEN step.
    public static User Create(string name, string email, string timeZone, string passwordHash) => new();

    public static string NormalizeEmail(string email) => email;
}
