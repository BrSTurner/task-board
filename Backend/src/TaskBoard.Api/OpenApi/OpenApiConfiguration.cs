using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace TaskBoard.Api.OpenApi;

public static class OpenApiConfiguration
{
    public const string BearerSchemeName = "Bearer";

    public static IServiceCollection AddTaskBoardOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            // NSwag 14 does not fully understand OpenAPI 3.1 (e.g. type arrays for nullability), so emit 3.0.
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "TaskBoard API";
                document.Info.Description = "Task management (/api/tasks) and user management (/api/auth) modules.";

                // Without this, the request host is baked into generated clients as their default base URL.
                document.Servers = [];

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT access token returned by POST /api/auth/login.",
                };
                return Task.CompletedTask;
            });

            // DTOs are closed shapes. Without this, NSwag adds "[key: string]: any" to every TypeScript interface,
            // which hides typos in property names.
            options.AddSchemaTransformer((schema, _, _) =>
            {
                if (schema.Properties is { Count: > 0 } && schema.AdditionalProperties is null)
                {
                    schema.AdditionalPropertiesAllowed = false;
                }

                return Task.CompletedTask;
            });

            // Only endpoints that actually require authorization get a security requirement.
            options.AddOperationTransformer((operation, context, _) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                var requiresAuth = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();
                if (requiresAuth)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerSchemeName, context.Document)] = [],
                    });
                }
                return Task.CompletedTask;
            });
        });
}
