using FluentAssertions;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.ValueObjects;
using Xunit;

namespace UserManagement.UnitTests.Domain;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnEmailInstance()
    {
        var email = Email.Create("user@example.com");
        email.Value.Should().Be("user@example.com");
    }

    [Theory]
    [InlineData("invalido")]
    [InlineData("sem-arroba.com")]
    [InlineData("@semlocal.com")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string value)
    {
        var act = () => Email.Create(value);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNull_ShouldThrowDomainException(string? value)
    {
        var act = () => Email.Create(value!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        var email = Email.Create("USER@EXAMPLE.COM");
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void TwoEmailsWithSameValue_ShouldBeEqual()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("user@example.com");
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}