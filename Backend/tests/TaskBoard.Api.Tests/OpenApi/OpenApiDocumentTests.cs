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
