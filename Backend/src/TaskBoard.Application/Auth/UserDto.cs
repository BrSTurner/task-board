using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Auth;

public sealed class UserDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required string TimeZone { get; init; }

    public static UserDto From(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        TimeZone = user.TimeZone,
    };
}
