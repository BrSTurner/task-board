using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskBoard.Api.Tests.Tasks;

public sealed class TaskEndpointsTests(TaskBoardApiFactory factory) : IClassFixture<TaskBoardApiFactory>
{
    private const string FutureLocal = "2099-01-15T09:30";

    private static async Task<JsonElement> CreateTaskAsync(HttpClient client, string title, string dueDate = FutureLocal, string? description = null)
    {
        var response = await client.PostJsonAsync("/api/tasks", new { title, description, dueDate });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadSuccessDataAsync();
    }

    private static string Id(JsonElement task) => task.GetProperty("id").GetString()!;

    [Fact]
    public async Task Create_returns_201_with_location_default_status_and_converted_due_date()
    {
        // Scenarios 8 and 21.
        var client = await factory.CreateAuthenticatedClientAsync("America/Sao_Paulo");

        var response = await client.PostJsonAsync("/api/tasks", new
        {
            title = "Prepare technical interview",
            description = "Review Clean Architecture",
            dueDate = "2026-10-21T15:00",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.ReadSuccessDataAsync();
        Assert.Equal($"/api/tasks/{Id(task)}", response.Headers.Location?.OriginalString);
        Assert.Equal("todo", task.GetProperty("status").GetString());
        Assert.Equal("2026-10-21T18:00:00Z", task.GetProperty("dueDateUtc").GetString());
        Assert.Equal("2026-10-21T15:00:00", task.GetProperty("dueDateLocal").GetString());
        Assert.False(task.TryGetProperty("userId", out _));

        var fetched = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
        Assert.Equal("Prepare technical interview", (await fetched.ReadSuccessDataAsync()).GetProperty("title").GetString());
    }

    [Fact]
    public async Task Invalid_task_returns_400_with_field_messages_and_persists_nothing()
    {
        // Scenarios 10, 11 and 12.
        var client = await factory.CreateAuthenticatedClientAsync();

        var missing = await client.PostJsonAsync("/api/tasks", new { title = " ", description = (string?)null });
        var tooLong = await client.PostJsonAsync("/api/tasks", new { title = new string('t', 51), description = new string('d', 401), dueDate = FutureLocal });

        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        var missingErrors = (await missing.ReadErrorAsync()).GetProperty("validationErrors");
        Assert.Equal("Title is required", missingErrors.GetProperty("title")[0].GetString());
        Assert.Equal("Due date is required", missingErrors.GetProperty("dueDate")[0].GetString());

        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        var lengthErrors = (await tooLong.ReadErrorAsync()).GetProperty("validationErrors");
        Assert.Equal("Title must be at most 50 characters", lengthErrors.GetProperty("title")[0].GetString());
        Assert.Equal("Description must be at most 400 characters", lengthErrors.GetProperty("description")[0].GetString());

        var list = await client.GetAsync("/api/tasks");
        Assert.Equal(0, (await list.ReadSuccessDataAsync()).GetArrayLength());
    }

    [Theory]
    [InlineData("2026-10-21T15:00:00Z")]
    [InlineData("2026-10-21T15:00:00+02:00")]
    public async Task Due_date_with_offset_is_rejected(string dueDate)
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostJsonAsync("/api/tasks", new { title = "Offset", dueDate });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True((await response.ReadErrorAsync()).GetProperty("validationErrors").TryGetProperty("dueDate", out _));
    }

    [Fact]
    public async Task List_returns_only_my_tasks_with_overdue_first()
    {
        // Scenarios 17, 18 and 24.
        var me = await factory.CreateAuthenticatedClientAsync();
        var someoneElse = await factory.CreateAuthenticatedClientAsync();
        await CreateTaskAsync(someoneElse, "Not mine");
        await CreateTaskAsync(me, "Future", "2099-01-01T10:00");
        await CreateTaskAsync(me, "Overdue later", "2001-06-01T10:00");
        await CreateTaskAsync(me, "Overdue earlier", "2001-01-01T10:00");

        var tasks = (await (await me.GetAsync("/api/tasks")).ReadSuccessDataAsync()).EnumerateArray().ToList();

        Assert.Equal(["Overdue earlier", "Overdue later", "Future"], tasks.Select(t => t.GetProperty("title").GetString()));
        Assert.Equal([true, true, false], tasks.Select(t => t.GetProperty("isOverdue").GetBoolean()));
    }

    [Fact]
    public async Task Moving_an_overdue_task_to_done_persists_and_clears_overdue()
    {
        // Scenarios 14, 15 and 20.
        var client = await factory.CreateAuthenticatedClientAsync();
        var task = await CreateTaskAsync(client, "Late", "2001-01-01T10:00");

        var moved = await client.PatchAsJsonAsync($"/api/tasks/{Id(task)}/status", new { status = "done" });

        Assert.Equal(HttpStatusCode.OK, moved.StatusCode);
        var movedTask = await moved.ReadSuccessDataAsync();
        Assert.Equal("done", movedTask.GetProperty("status").GetString());
        Assert.False(movedTask.GetProperty("isOverdue").GetBoolean());

        var reloaded = await (await client.GetAsync($"/api/tasks/{Id(task)}")).ReadSuccessDataAsync();
        Assert.Equal("done", reloaded.GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("\"archived\"")]
    [InlineData("2")]
    [InlineData("null")]
    public async Task Invalid_status_values_return_400_envelope(string statusJson)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var task = await CreateTaskAsync(client, "Status");

        var content = new StringContent($"{{ \"status\": {statusJson} }}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/tasks/{Id(task)}/status", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await response.ReadErrorAsync();
    }

    [Fact]
    public async Task Put_replaces_editable_fields()
    {
        // Scenario 13.
        var client = await factory.CreateAuthenticatedClientAsync("Asia/Tokyo");
        var task = await CreateTaskAsync(client, "Original", description: "Old");

        var response = await client.PutAsJsonAsync($"/api/tasks/{Id(task)}", new
        {
            title = "Updated",
            description = (string?)null,
            dueDate = "2099-02-01T08:00",
            status = "inProgress",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.ReadSuccessDataAsync();
        Assert.Equal("Updated", updated.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, updated.GetProperty("description").ValueKind);
        Assert.Equal("inProgress", updated.GetProperty("status").GetString());
        Assert.Equal("2099-01-31T23:00:00Z", updated.GetProperty("dueDateUtc").GetString());
        Assert.Equal("2099-02-01T08:00:00", updated.GetProperty("dueDateLocal").GetString());
    }

    [Fact]
    public async Task Delete_returns_204_without_body_and_the_task_is_gone()
    {
        // Scenario 16.
        var client = await factory.CreateAuthenticatedClientAsync();
        var task = await CreateTaskAsync(client, "Disposable");

        var response = await client.DeleteAsync($"/api/tasks/{Id(task)}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/tasks/{Id(task)}")).StatusCode);
    }

    [Fact]
    public async Task Another_users_task_is_404_for_every_verb_and_remains_unchanged()
    {
        // Scenario 24.
        var owner = await factory.CreateAuthenticatedClientAsync();
        var intruder = await factory.CreateAuthenticatedClientAsync();
        var task = await CreateTaskAsync(owner, "Private");
        var url = $"/api/tasks/{Id(task)}";

        var responses = new[]
        {
            await intruder.GetAsync(url),
            await intruder.PutAsJsonAsync(url, new { title = "Hacked", dueDate = FutureLocal, status = "done" }),
            await intruder.PatchAsJsonAsync($"{url}/status", new { status = "done" }),
            await intruder.DeleteAsync(url),
        };

        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var error = await response.ReadErrorAsync();
            Assert.Equal("Task.NotFound", error.GetProperty("code").GetString());
        }

        var unchanged = await (await owner.GetAsync(url)).ReadSuccessDataAsync();
        Assert.Equal("Private", unchanged.GetProperty("title").GetString());
        Assert.Equal("todo", unchanged.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Non_guid_id_returns_404_envelope()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/tasks/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await response.ReadErrorAsync();
    }

    public static TheoryData<string, string> ProtectedRoutes => new()
    {
        { "GET", "/api/tasks" },
        { "GET", $"/api/tasks/{Guid.Empty}" },
        { "POST", "/api/tasks" },
        { "PUT", $"/api/tasks/{Guid.Empty}" },
        { "PATCH", $"/api/tasks/{Guid.Empty}/status" },
        { "DELETE", $"/api/tasks/{Guid.Empty}" },
    };

    [Theory]
    [MemberData(nameof(ProtectedRoutes))]
    public async Task Task_routes_require_authentication(string method, string url)
    {
        // Scenario 25.
        using var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = method is "POST" or "PUT" or "PATCH" ? JsonContent.Create(new { }) : null,
        };

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.ReadErrorAsync();
        Assert.Equal("Unauthorized", error.GetProperty("code").GetString());
    }
}
