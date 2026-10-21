namespace TaskBoard.Application.Abstractions;

/// <summary>The authenticated caller, taken from the validated access token — never from request bodies.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }

    /// <summary>The caller's IANA timezone id (token claim <c>zoneinfo</c>).</summary>
    string TimeZoneId { get; }
}
