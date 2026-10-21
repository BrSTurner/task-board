using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.Application.Auth;
using TaskBoard.Application.Tasks;

namespace TaskBoard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>(ServiceLifetime.Singleton);

        // Use cases are plain classes (no interface per use case, no mediator).
        services.AddScoped<RegisterUser>();
        services.AddScoped<LoginUser>();
        services.AddScoped<GetCurrentUser>();

        services.AddScoped<GetMyTasks>();
        services.AddScoped<GetTaskById>();
        services.AddScoped<CreateTask>();
        services.AddScoped<UpdateTask>();
        services.AddScoped<ChangeTaskStatus>();
        services.AddScoped<DeleteTask>();

        return services;
    }
}
