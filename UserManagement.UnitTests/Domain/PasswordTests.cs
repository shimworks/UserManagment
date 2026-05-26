using FluentAssertions;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.ValueObjects;
using Xunit;

namespace UserManagement.UnitTests.Domain;

public sealed class PasswordTests
{
    [Fact]
    public void ToString_ShouldReturnProtectedString()
    {
        var password = Password.FromHash("$2a$12$somehash");
        password.ToString().Should().Be("[PROTECTED]");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FromHash_WithEmptyOrNull_ShouldThrowDomainException(string? hash)
    {
        var act = () => Password.FromHash(hash!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TwoPasswordsWithSameHash_ShouldBeEqual()
    {
        var hash = "$2a$12$somehash";
        var a = Password.FromHash(hash);
        var b = Password.FromHash(hash);
        a.Should().Be(b);
    }
}