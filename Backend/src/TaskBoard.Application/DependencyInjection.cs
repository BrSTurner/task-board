using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.Application.Auth;

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

        return services;
    }
}
