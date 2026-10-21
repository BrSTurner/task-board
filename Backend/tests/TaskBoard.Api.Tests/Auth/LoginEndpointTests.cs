using System.Net;
using System.Net.Http.Headers;

namespace TaskBoard.Api.Tests.Auth;

public sealed class LoginEndpointTests(TaskBoardApiFactory factory) : IClassFixture<TaskBoardApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task RegisterAsync(string email) =>
        await _client.PostJsonAsync("/api/auth/register", new { name = "Dave", email, timeZone = "Asia/Tokyo", password = "DavePass123!" });

    [Fact]
    public async Task Valid_login_returns_bearer_token_that_authorizes_me()
    {
        await RegisterAsync("dave.login@example.test");

        var login = await _client.PostJsonAsync("/api/auth/login", new { email = "Dave.Login@example.test", password = "DavePass123!" });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var data = await login.ReadSuccessDataAsync();
        var token = data.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrEmpty(token));
        Assert.True(data.GetProperty("expiresAt").GetDateTime() > DateTime.UtcNow);
        Assert.Equal("Asia/Tokyo", data.GetProperty("user").GetProperty("timeZone").GetString());

        using var me = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        me.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _client.SendAsync(me);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.Equal("dave.login@example.test", (await meResponse.ReadSuccessDataAsync()).GetProperty("email").GetString());
    }

    [Fact]
    public async Task Wrong_password_returns_401_envelope()
    {
        await RegisterAsync("dave.wrong@example.test");

        var login = await _client.PostJsonAsync("/api/auth/login", new { email = "dave.wrong@example.test", password = "Nope1234!" });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        var error = await login.ReadErrorAsync();
        Assert.Equal("Auth.InvalidCredentials", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Me_without_token_returns_401_envelope()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Unauthorized", (await response.ReadErrorAsync()).GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("not-a-jwt")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMifQ.invalid-signature")]
    public async Task Me_with_invalid_token_returns_401_envelope(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Unauthorized", (await response.ReadErrorAsync()).GetProperty("code").GetString());
    }
}
