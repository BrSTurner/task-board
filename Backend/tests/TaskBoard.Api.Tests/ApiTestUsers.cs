using System.Net;
using System.Net.Http.Headers;

namespace TaskBoard.Api.Tests;

internal static class ApiTestUsers
{
    /// <summary>Registers a unique user and returns a client that sends its bearer token.</summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(this TaskBoardApiFactory factory, string timeZone = "America/Sao_Paulo")
    {
        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.test";
        const string password = "TestPass123!";

        var register = await client.PostJsonAsync("/api/auth/register", new { name = "Test User", email, timeZone, password });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostJsonAsync("/api/auth/login", new { email, password });
        var token = (await login.ReadSuccessDataAsync()).GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
