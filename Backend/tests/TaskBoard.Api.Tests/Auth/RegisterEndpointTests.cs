using System.Net;
using System.Text;

namespace TaskBoard.Api.Tests.Auth;

public sealed class RegisterEndpointTests(TaskBoardApiFactory factory) : IClassFixture<TaskBoardApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object ValidBody(string email) => new
    {
        name = "Carol Smith",
        email,
        timeZone = "Europe/London",
        password = "CarolPass123!",
    };

    [Fact]
    public async Task Valid_registration_returns_201_with_safe_user_dto()
    {
        var response = await _client.PostJsonAsync("/api/auth/register", ValidBody("Carol.Register@Example.test"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = await response.ReadSuccessDataAsync();
        Assert.True(Guid.TryParse(data.GetProperty("id").GetString(), out _));
        Assert.Equal("Carol Smith", data.GetProperty("name").GetString());
        Assert.Equal("carol.register@example.test", data.GetProperty("email").GetString());
        Assert.Equal("Europe/London", data.GetProperty("timeZone").GetString());
        Assert.False(data.TryGetProperty("password", out _));
        Assert.False(data.TryGetProperty("passwordHash", out _));
    }

    [Fact]
    public async Task Duplicate_email_returns_409_envelope()
    {
        await _client.PostJsonAsync("/api/auth/register", ValidBody("carol.duplicate@example.test"));

        var response = await _client.PostJsonAsync("/api/auth/register", ValidBody("  CAROL.DUPLICATE@example.test "));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.ReadErrorAsync();
        Assert.Equal("Auth.EmailTaken", error.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Invalid_registration_returns_400_with_camel_case_validation_errors()
    {
        var response = await _client.PostJsonAsync("/api/auth/register", new { name = "", email = "nope", timeZone = "Nowhere/City", password = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.ReadErrorAsync()).GetProperty("validationErrors");
        Assert.Equal("Name is required", errors.GetProperty("name")[0].GetString());
        Assert.True(errors.TryGetProperty("email", out _));
        Assert.True(errors.TryGetProperty("timeZone", out _));
        Assert.True(errors.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Malformed_json_returns_400_envelope()
    {
        var content = new StringContent("{ \"name\": ", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("BadRequest", (await response.ReadErrorAsync()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Non_json_content_type_returns_415_envelope()
    {
        var content = new StringContent("name=Carol", Encoding.UTF8, "text/plain");

        var response = await _client.PostAsync("/api/auth/register", content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Equal("UnsupportedMediaType", (await response.ReadErrorAsync()).GetProperty("code").GetString());
    }
}
