using TaskBoard.Api.Endpoints;
using TaskBoard.Api.Http;
using TaskBoard.Api.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddTaskBoardErrorHandling();
builder.Services.AddTaskBoardOpenApi();

var app = builder.Build();

app.UseTaskBoardErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPublicEndpoints();

app.Run();

public partial class Program;
