using System.Net;
using System.Text.Json;

namespace TaskBoard.Api.Tests.OpenApi;

public sealed class OpenApiDocumentTests(TaskBoardApiFactory factory)
    : IClassFixture<TaskBoardApiFactory>
{
    private async Task<JsonDocument> GetDocumentAsync()
    {
        var response = await factory.CreateClient().GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Document_uses_openapi_3_0_for_nswag_compatibility()
    {
        using var doc = await GetDocumentAsync();

        Assert.StartsWith("3.0", doc.RootElement.GetProperty("openapi").GetString());
    }

    [Fact]
    public async Task Document_exposes_operation_ids_and_bearer_scheme_without_servers()
    {
        using var doc = await GetDocumentAsync();
        var root = doc.RootElement;

        Assert.Equal("Ping", root.GetProperty("paths").GetProperty("/api/public/ping").GetProperty("get").GetProperty("operationId").GetString());
        Assert.Equal("bearer", root.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer").GetProperty("scheme").GetString());
        Assert.False(root.TryGetProperty("servers", out _), "servers must not be baked into generated clients");
    }

    [Fact]
    public async Task Anonymous_operations_have_no_security_requirement()
    {
        using var doc = await GetDocumentAsync();

        var ping = doc.RootElement.GetProperty("paths").GetProperty("/api/public/ping").GetProperty("get");
        Assert.False(ping.TryGetProperty("security", out _));
    }

    [Theory]
    [InlineData("/api/tasks", "get")]
    [InlineData("/api/tasks", "post")]
    [InlineData("/api/tasks/{id}", "put")]
    [InlineData("/api/tasks/{id}/status", "patch")]
    [InlineData("/api/tasks/{id}", "delete")]
    [InlineData("/api/auth/me", "get")]
    public async Task Protected_operations_declare_the_bearer_requirement(string path, string method)
    {
        using var doc = await GetDocumentAsync();

        var operation = doc.RootElement.GetProperty("paths").GetProperty(path).GetProperty(method);

        Assert.True(operation.GetProperty("security")[0].TryGetProperty("Bearer", out _));
    }

    [Fact]
    public async Task Task_contract_uses_string_statuses_and_required_response_fields()
    {
        using var doc = await GetDocumentAsync();
        var schemas = doc.RootElement.GetProperty("components").GetProperty("schemas");

        var statuses = schemas.GetProperty("TaskItemStatus").GetProperty("enum").EnumerateArray().Select(e => e.GetString()!).ToArray();
        var required = schemas.GetProperty("TaskDto").GetProperty("required").EnumerateArray().Select(e => e.GetString()!).ToArray();

        Assert.Equal(["todo", "inProgress", "done"], statuses);
        Assert.Equal(["id", "title", "description", "status", "dueDateUtc", "dueDateLocal", "isOverdue"], required);
        Assert.True(doc.RootElement.GetProperty("paths").GetProperty("/api/tasks/{id}").GetProperty("delete").GetProperty("responses").TryGetProperty("204", out _));
    }

    [Theory]
    [InlineData("TaskDto")]
    [InlineData("CreateTaskRequest")]
    [InlineData("ApiResult")]
    public async Task Dto_schemas_forbid_additional_properties_so_generated_clients_are_strict(string schema)
    {
        using var doc = await GetDocumentAsync();

        var additional = doc.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(schema).GetProperty("additionalProperties");

        Assert.Equal(JsonValueKind.False, additional.ValueKind);
    }

    [Fact]
    public async Task Request_contract_marks_register_fields_as_required()
    {
        using var doc = await GetDocumentAsync();

        var required = doc.RootElement.GetProperty("components").GetProperty("schemas")
            .GetProperty("RegisterUserRequest").GetProperty("required")
            .EnumerateArray().Select(e => e.GetString()!).ToArray();

        Assert.Equal(["name", "email", "timeZone", "password"], required);
    }
}
