namespace TaskBoard.Infrastructure.Persistence.Seeding;

/// <summary>Demo data applied through EF Core <c>HasData</c>, i.e. as part of the migrations.</summary>
public static class SeedData
{
    public const string DemoPassword = "DemoPass123!";

    public static readonly Guid AliceId = Guid.Parse("a11ce000-0000-7000-8000-000000000001");
    public static readonly Guid BobId = Guid.Parse("b0b00000-0000-7000-8000-000000000002");

    // TDD stub — data is added in the GREEN step.
    internal static object[] Users => [];

    internal static object[] Tasks => [];
}
