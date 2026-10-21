using TaskBoard.Application.Tasks;

namespace TaskBoard.Application.Tests.Tasks;

public sealed class DueDatesTests
{
    private static readonly TimeZoneInfo SaoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    private static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    [Fact]
    public void Local_wall_clock_is_converted_to_utc_using_the_users_timezone()
    {
        // Scenario 21: 2026-10-21 15:00 in America/Sao_Paulo is stored as 2026-10-21T18:00:00Z.
        var result = DueDates.ToUtc(new DateTime(2026, 10, 21, 15, 0, 0), SaoPaulo);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2026, 10, 21, 18, 0, 0), result.Value);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Fact]
    public void Utc_is_displayed_as_wall_clock_in_the_users_timezone()
    {
        // Scenario 22.
        var local = DueDates.ToLocal(new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc), SaoPaulo);

        Assert.Equal(new DateTime(2026, 10, 21, 15, 0, 0), local);
        Assert.Equal(DateTimeKind.Unspecified, local.Kind); // serialized without "Z" or offset
    }

    [Fact]
    public void Local_time_inside_a_daylight_saving_gap_is_a_validation_error()
    {
        // New York springs forward on 2026-03-08: 02:00–02:59 does not exist.
        var result = DueDates.ToUtc(new DateTime(2026, 3, 8, 2, 30, 0), NewYork);

        Assert.False(result.IsSuccess);
        Assert.True(result.Error!.ValidationErrors!.ContainsKey("dueDate"));
    }

    [Fact]
    public void Ambiguous_local_time_resolves_to_standard_time()
    {
        // New York falls back on 2026-11-01: 01:30 happens twice; .NET picks standard time (UTC-5).
        var result = DueDates.ToUtc(new DateTime(2026, 11, 1, 1, 30, 0), NewYork);

        Assert.Equal(new DateTime(2026, 11, 1, 6, 30, 0), result.Value);
    }
}
