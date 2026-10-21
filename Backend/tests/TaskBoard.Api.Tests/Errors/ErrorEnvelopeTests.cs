using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace TaskBoard.Api.Tests.Errors;

public sealed class ErrorEnvelopeTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Unknown_route_returns_404_envelope()
    {
        var response = await factory.CreateClient().GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("NotFound", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Unhandled_exception_returns_500_envelope_without_internals()
    {
        var client = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                services.AddSingleton<IStartupFilter, ThrowingEndpointStartupFilter>()))
            .CreateClient();

        var response = await client.GetAsync("/__test/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret internal detail", body);
        var error = await ReadErrorAsync(response, body);
        Assert.Equal("ServerError", error.GetProperty("code").GetString());
    }

    private static async Task<JsonElement> ReadErrorAsync(HttpResponseMessage response, string? body = null)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(body ?? await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("isSuccess").GetBoolean());
        return json.RootElement.GetProperty("error").Clone();
    }

    private sealed class ThrowingEndpointStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);
            // Appended after the real pipeline so the exception bubbles through the app's exception handler.
            app.Map("/__test/throw", b => b.Run(_ => throw new InvalidOperationException("secret internal detail")));
        };
    }
}
