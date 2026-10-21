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

        return app;
    }
}
