using TaskBoard.Api.Http;
using TaskBoard.Application.Auth;

namespace TaskBoard.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterUserRequest request, RegisterUser registerUser, CancellationToken cancellationToken) =>
                (await registerUser.ExecuteAsync(request, cancellationToken))
                    // No Location header: there is no "get user by id" resource to point to.
                    .ToHttpResult(user => TypedResults.Created((string?)null, ApiResult.Success(user))))
            .AllowAnonymous()
            .WithValidation<RegisterUserRequest>()
            .WithName("Register")
            .WithSummary("Creates a new account. Does not sign the user in.")
            .Produces<ApiResult<UserDto>>(StatusCodes.Status201Created)
            .ProducesApiError(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginUserRequest request, LoginUser loginUser, CancellationToken cancellationToken) =>
                (await loginUser.ExecuteAsync(request, cancellationToken)).ToHttpResult(ApiResults.Ok))
            .AllowAnonymous()
            .WithValidation<LoginUserRequest>()
            .WithName("Login")
            .WithSummary("Exchanges email and password for a bearer access token.")
            .Produces<ApiResult<LoginResponse>>()
            .ProducesApiError(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", async (GetCurrentUser getCurrentUser, CancellationToken cancellationToken) =>
                (await getCurrentUser.ExecuteAsync(cancellationToken)).ToHttpResult(ApiResults.Ok))
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("Returns the authenticated user.")
            .Produces<ApiResult<UserDto>>()
            .ProducesApiError(StatusCodes.Status401Unauthorized);

        return app;
    }
}
