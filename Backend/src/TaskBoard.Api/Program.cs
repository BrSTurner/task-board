using TaskBoard.Api.Endpoints;
using TaskBoard.Api.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddTaskBoardOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPublicEndpoints();

app.Run();

public partial class Program;
