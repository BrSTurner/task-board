using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tests.Common;

public sealed class TimeZonesTests
{
    [Theory]
    [InlineData("America/Sao_Paulo")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Europe/London")]
    [InlineData("UTC")]
    public void Accepts_iana_ids(string id)
    {
        Assert.True(TimeZones.TryFindIana(id, out var tz));
        Assert.NotNull(tz);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Mars/Olympus_Mons")]
    [InlineData("E. South America Standard Time")] // Windows id, not IANA
    public void Rejects_unknown_or_non_iana_ids(string? id)
    {
        Assert.False(TimeZones.TryFindIana(id, out _));
    }
}
