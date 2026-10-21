using System.Net;
using System.Net.Http.Headers;

namespace TaskBoard.Api.Tests.Auth;

/// <summary>The seeded demo accounts work end-to-end and only see their own tasks.</summary>
public sealed class DemoSeedEndpointTests(TaskBoardApiFactory factory) : IClassFixture<TaskBoardApiFactory>
{
    private async Task<HttpClient> LoginAsync(string email)
    {
        var client = factory.CreateClient();
        var login = await client.PostJsonAsync("/api/auth/login", new { email, password = "DemoPass123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.ReadSuccessDataAsync()).GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Demo_user_sees_only_their_seeded_tasks_in_their_timezone()
    {
        var alice = await LoginAsync("demo@example.test");

        var tasks = (await (await alice.GetAsync("/api/tasks")).ReadSuccessDataAsync()).EnumerateArray().ToList();

        Assert.Equal(8, tasks.Count);
        var interview = tasks.Single(t => t.GetProperty("title").GetString() == "Prepare technical interview");
        Assert.Equal("2026-10-21T18:00:00Z", interview.GetProperty("dueDateUtc").GetString());
        Assert.Equal("2026-10-21T15:00:00", interview.GetProperty("dueDateLocal").GetString());
        Assert.DoesNotContain(tasks, t => t.GetProperty("title").GetString() == "Review pull requests");
    }

    [Fact]
    public async Task Second_demo_user_cannot_read_the_first_users_task()
    {
        var bob = await LoginAsync("BOB@example.test");

        var response = await bob.GetAsync("/api/tasks/a11ce000-7a5c-7000-8000-000000000001");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(3, (await (await bob.GetAsync("/api/tasks")).ReadSuccessDataAsync()).GetArrayLength());
    }
}
