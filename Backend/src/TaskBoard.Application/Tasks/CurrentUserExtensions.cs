using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

internal static class CurrentUserExtensions
{
    /// <summary>The timezone was validated at registration; an unknown id means a tampered or stale token.</summary>
    public static TimeZoneInfo GetTimeZone(this ICurrentUser currentUser) =>
        TimeZones.TryFindIana(currentUser.TimeZoneId, out var timeZone)
            ? timeZone
            : throw new UnauthorizedAccessException("The access token has an unknown timezone.");
}
