using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Exceptions;

namespace TaskBoard.Domain.Tests.Entities;

public sealed class UserTests
{
    private const string Hash = "AQAAAAIAAYagAAAAE-fake-hash";

    [Fact]
    public void Create_generates_id_and_normalizes_values()
    {
        var user = User.Create("  Alice Martins ", "  Alice@Example.TEST ", "America/Sao_Paulo", Hash);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alice Martins", user.Name);
        Assert.Equal("alice@example.test", user.Email);
        Assert.Equal("America/Sao_Paulo", user.TimeZone);
        Assert.Equal(Hash, user.PasswordHash);
    }

    [Fact]
    public void Create_generates_a_different_id_for_each_user()
    {
        var first = User.Create("A", "a@example.test", "UTC", Hash);
        var second = User.Create("B", "b@example.test", "UTC", Hash);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Theory]
    [InlineData("", "a@example.test", "UTC", Hash)]
    [InlineData("   ", "a@example.test", "UTC", Hash)]
    [InlineData("Alice", " ", "UTC", Hash)]
    [InlineData("Alice", "a@example.test", "", Hash)]
    [InlineData("Alice", "a@example.test", "UTC", "")]
    public void Create_rejects_missing_required_values(string name, string email, string timeZone, string hash)
    {
        Assert.Throws<DomainException>(() => User.Create(name, email, timeZone, hash));
    }

    [Fact]
    public void Create_rejects_name_longer_than_max_length()
    {
        var name = new string('a', User.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => User.Create(name, "a@example.test", "UTC", Hash));
    }

    [Fact]
    public void Create_rejects_email_longer_than_max_length()
    {
        var email = new string('a', User.EmailMaxLength - "@example.test".Length + 1) + "@example.test";

        Assert.Throws<DomainException>(() => User.Create("Alice", email, "UTC", Hash));
    }

    [Theory]
    [InlineData(" Bob@Example.Test ", "bob@example.test")]
    [InlineData("BOB@EXAMPLE.TEST", "bob@example.test")]
    public void NormalizeEmail_trims_and_lowercases(string input, string expected)
    {
        Assert.Equal(expected, User.NormalizeEmail(input));
    }
}
