namespace TaskBoard.Application.Common;

public static class TimeZones
{
    /// <summary>Resolves an IANA timezone id. Windows ids (e.g. "E. South America Standard Time") are rejected.</summary>
    public static bool TryFindIana(string? id, out TimeZoneInfo timeZone)
    {
        timeZone = TimeZoneInfo.Utc;
        return false; // TDD stub
    }
}
