using System.Text.Json;
using System.Text.Json.Serialization;
using TaskBoard.Api.Endpoints;
using TaskBoard.Api.Http;
using TaskBoard.Api.OpenApi;
using TaskBoard.Application;
using TaskBoard.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.ConfigureHttpJsonOptions(options =>
    // Enums travel as camelCase strings (todo, inProgress, done); numeric values are rejected.
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);
builder.Services.AddTaskBoardErrorHandling();
builder.Services.AddTaskBoardOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Creates/updates the SQLite schema and applies the HasData seed. Outside Development, run
    // `dotnet ef database update` as an explicit deployment step instead.
    await app.Services.MigrateDatabaseAsync();
}

app.UseTaskBoardErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPublicEndpoints();
app.MapAuthEndpoints();

app.Run();

public partial class Program;
