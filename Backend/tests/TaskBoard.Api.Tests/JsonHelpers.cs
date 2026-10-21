using System.Net.Http.Json;
using System.Text.Json;

namespace TaskBoard.Api.Tests;

internal static class JsonHelpers
{
    /// <summary>Reads the raw JSON so tests assert the actual wire contract (camelCase, envelope shape).</summary>
    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.Clone();
    }

    public static async Task<JsonElement> ReadSuccessDataAsync(this HttpResponseMessage response)
    {
        var root = await response.ReadJsonAsync();
        Assert.True(root.GetProperty("isSuccess").GetBoolean(), root.ToString());
        return root.GetProperty("data");
    }

    public static async Task<JsonElement> ReadErrorAsync(this HttpResponseMessage response)
    {
        var root = await response.ReadJsonAsync();
        Assert.False(root.GetProperty("isSuccess").GetBoolean(), root.ToString());
        return root.GetProperty("error");
    }

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body) =>
        client.PostAsJsonAsync(url, body);
}
