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

    /// <summary>Always stored normalized (trimmed, lower-case) so uniqueness is case-insensitive.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>IANA timezone id (validated by the application layer), e.g. <c>America/Sao_Paulo</c>.</summary>
    public string TimeZone { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public static User Create(string name, string email, string timeZone, string passwordHash) => new()
    {
        Id = Guid.CreateVersion7(),
        Name = Guard.RequiredText(name, nameof(Name), NameMaxLength),
        Email = Guard.RequiredText(NormalizeEmail(email), nameof(Email), EmailMaxLength),
        TimeZone = Guard.RequiredText(timeZone, nameof(TimeZone), TimeZoneMaxLength),
        PasswordHash = Guard.RequiredText(passwordHash, nameof(PasswordHash), int.MaxValue),
    };

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
