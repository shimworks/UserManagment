using FluentAssertions;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.ValueObjects;
using Xunit;

namespace UserManagement.UnitTests.Domain;

public sealed class UserTests
{
    private static User BuildValidUser() =>
        User.Create(
            "John Doe",
            Email.Create("john@example.com"),
            Password.FromHash("$2a$12$hashedpassword"),
            UserRole.Customer);

    [Fact]
    public void Create_WithValidData_ShouldReturnActiveUser()
    {
        var user = BuildValidUser();

        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowDomainException(string name)
    {
        var act = () => User.Create(name,
            Email.Create("john@example.com"),
            Password.FromHash("$2a$12$hashedpassword"),
            UserRole.Customer);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalseAndUpdateTimestamp()
    {
        var user = BuildValidUser();
        var beforeUpdate = user.UpdatedAt;

        // Pequeno delay para garantir diferenca no timestamp
        Thread.Sleep(10);
        user.Deactivate();

        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveTrue()
    {
        var user = BuildValidUser();
        user.Deactivate();
        user.Activate();
        user.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeName_WithEmptyValue_ShouldThrowDomainException(string newName)
    {
        var user = BuildValidUser();
        var act = () => user.ChangeName(newName);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeName_WithValidValue_ShouldUpdateNameAndTimestamp()
    {
        var user = BuildValidUser();
        Thread.Sleep(10);
        user.ChangeName("Jane Doe");

        user.Name.Should().Be("Jane Doe");
        user.UpdatedAt.Should().BeAfter(user.CreatedAt);
    }
}