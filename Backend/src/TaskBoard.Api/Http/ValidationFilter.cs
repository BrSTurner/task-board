using System.Text.Json;
using FluentValidation;

namespace TaskBoard.Api.Http;

/// <summary>Runs the FluentValidation validator for <typeparamref name="TRequest"/> before the endpoint handler.</summary>
internal sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return ApiResults.Error(StatusCodes.Status400BadRequest, new ApiError { Code = "BadRequest", Message = "A request body is required." });
        }

        var validation = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (validation.IsValid)
        {
            return await next(context);
        }

        // Keys match the JSON property names the client sent (camelCase).
        var errors = validation.Errors
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return ApiResults.ValidationProblem(errors);
    }
}

internal static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValidationFilter<TRequest>>()
            .ProducesApiError(StatusCodes.Status400BadRequest);
}
