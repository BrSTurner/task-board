using System.Net;
using System.Text.Json;

namespace TaskBoard.Api.Tests.Public;

public sealed class PingEndpointTests(TaskBoardApiFactory factory)
    : IClassFixture<TaskBoardApiFactory>
{
    [Fact]
    public async Task Ping_is_anonymous_and_returns_success_envelope()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/public/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("ok", json.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("error").ValueKind);
    }
}
