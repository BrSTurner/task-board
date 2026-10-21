using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

/// <summary>Converts between the user's local wall-clock time and the stored UTC instant (BR-008).</summary>
public static class DueDates
{
    public static Result<DateTime> ToUtc(DateTime local, TimeZoneInfo timeZone)
    {
        var wallClock = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(wallClock))
        {
            return Error.Validation("dueDate", "This time does not exist in your time zone (daylight saving time change). Choose a different time.");
        }

        // Ambiguous times (clocks falling back) resolve to standard time, as documented by TimeZoneInfo.
        return TimeZoneInfo.ConvertTimeToUtc(wallClock, timeZone);
    }

    /// <summary>Always returns <see cref="DateTimeKind.Unspecified"/> so it serializes without "Z" or an offset.</summary>
    public static DateTime ToLocal(DateTime utc, TimeZoneInfo timeZone) =>
        DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone), DateTimeKind.Unspecified);
}
