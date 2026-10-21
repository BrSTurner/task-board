namespace TaskBoard.Application.Common;

public static class TimeZones
{
    /// <summary>
    /// Resolves an IANA timezone id (e.g. <c>America/Sao_Paulo</c>). Windows ids such as
    /// "E. South America Standard Time" are rejected. On Windows this relies on ICU, so the app must not
    /// run with <c>InvariantGlobalization</c>.
    /// </summary>
    public static bool TryFindIana(string? id, out TimeZoneInfo timeZone)
    {
        if (!string.IsNullOrWhiteSpace(id)
            && TimeZoneInfo.TryFindSystemTimeZoneById(id, out var found)
            && found.HasIanaId)
        {
            timeZone = found;
            return true;
        }

        timeZone = TimeZoneInfo.Utc;
        return false;
    }
}
