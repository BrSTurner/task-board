using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

/// <summary>Converts between the user's local wall-clock time and the stored UTC instant (BR-008).</summary>
public static class DueDates
{
    // TDD stubs — implemented in the GREEN step.
    public static Result<DateTime> ToUtc(DateTime local, TimeZoneInfo timeZone) => local;

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo timeZone) => utc;
}
