using FluentAssertions;
using Moq;
using UserManagement.Application.Auth.Commands;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.ValueObjects;
using UserManagement.Application.Common.Interfaces;
using Xunit;

namespace UserManagement.UnitTests.Application;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private LoginCommandHandler CreateSut() => new(
        _repositoryMock.Object,
        _hasherMock.Object,
        _tokenServiceMock.Object);

    private static User BuildActiveUser() =>
        User.Create(
            "John Doe",
            Email.Create("john@example.com"),
            Password.FromHash("$2a$12$hashedpassword"),
            UserRole.Customer);

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = BuildActiveUser();
        var command = new LoginCommand("john@example.com", "Pass@1234");

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasherMock
            .Setup(h => h.Verify(command.Password, user.Password.Hash))
            .Returns(true);

        _tokenServiceMock
            .Setup(t => t.GenerateToken(user))
            .Returns("jwt.token.here");

        _tokenServiceMock
            .Setup(t => t.GetExpiration())
            .Returns(DateTime.UtcNow.AddHours(1));

        // Act
        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be("jwt.token.here");
        result.Role.Should().Be("Customer");
        _tokenServiceMock.Verify(t => t.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        var command = new LoginCommand("ghost@example.com", "Pass@1234");

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldThrowUnauthorizedException()
    {
        var user = BuildActiveUser();
        var command = new LoginCommand("john@example.com", "WrongPass");

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasherMock
            .Setup(h => h.Verify(command.Password, user.Password.Hash))
            .Returns(false);

        var act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        var user = BuildActiveUser();
        user.Deactivate();

        var command = new LoginCommand("john@example.com", "Pass@1234");

        _repositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await CreateSut().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }
}