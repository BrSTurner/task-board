using TaskBoard.Infrastructure.Authentication;

namespace TaskBoard.Infrastructure.Tests.Authentication;

public sealed class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

    [Fact]
    public void Hash_never_contains_plaintext_and_is_salted()
    {
        var first = _sut.Hash("DemoPass123!");
        var second = _sut.Hash("DemoPass123!");

        Assert.DoesNotContain("DemoPass123!", first);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_accepts_correct_password_and_rejects_others()
    {
        var hash = _sut.Hash("DemoPass123!");

        Assert.True(_sut.Verify(hash, "DemoPass123!"));
        Assert.False(_sut.Verify(hash, "demopass123!"));
        Assert.False(_sut.Verify(hash, ""));
    }

    [Fact]
    public void Verify_rejects_malformed_hash()
    {
        Assert.False(_sut.Verify("not-a-valid-hash", "DemoPass123!"));
    }
}
