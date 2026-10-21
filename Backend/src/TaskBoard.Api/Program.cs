using TaskBoard.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.MapPublicEndpoints();

app.Run();

public partial class Program;
