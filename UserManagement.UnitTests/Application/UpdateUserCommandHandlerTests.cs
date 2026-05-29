using AutoMapper;
using FluentAssertions;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Users.Commands;
using UserManagement.Application.Users.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.ValueObjects;
using Xunit;

namespace UserManagement.UnitTests.Application;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private UpdateUserCommandHandler CreateSut() => new(
        _repositoryMock.Object,
        _unitOfWorkMock.Object,
        _mapperMock.Object);

    private static User BuildUser(string email = "john@example.com") =>
        User.Create("John", Email.Create(email), Password.FromHash("$2a$12$hash"), UserRole.Customer);

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateNameAndCommit()
    {
        // Arrange
        var user = BuildUser();
        var command = new UpdateUserCommand(user.Id, "Jane", user.Email.Value);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<UserResponse>(It.IsAny<User>()))
            .Returns(new UserResponse(user.Id, command.Name, command.Email, "Customer", true, DateTime.UtcNow, DateTime.UtcNow));

        // Act
        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Jane");
        _repositoryMock.Verify(r => r.Update(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNewEmail_ShouldCheckConflictAndUpdate()
    {
        // Arrange
        var user = BuildUser("old@example.com");
        var command = new UpdateUserCommand(user.Id, "John", "new@example.com");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(m => m.Map<UserResponse>(It.IsAny<User>()))
            .Returns(new UserResponse(user.Id, "John", "new@example.com", "Customer", true, DateTime.UtcNow, DateTime.UtcNow));

        // Act
        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        result.Email.Should().Be("new@example.com");
        _repositoryMock.Verify(r => r.ExistsByEmailAsync("new@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateNewEmail_ShouldThrowConflictException()
    {
        // Arrange
        var user = BuildUser("old@example.com");
        var command = new UpdateUserCommand(user.Id, "John", "taken@example.com");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await CreateSut().Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await CreateSut().Handle(new UpdateUserCommand(id, "X", "x@x.com"), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithSameEmail_ShouldNotCheckConflict()
    {
        // Arrange — e-mail não mudou, ExistsByEmailAsync não deve ser chamado
        var user = BuildUser("john@example.com");
        var command = new UpdateUserCommand(user.Id, "John Updated", "john@example.com");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<UserResponse>(It.IsAny<User>()))
            .Returns(new UserResponse(user.Id, command.Name, command.Email, "Customer", true, DateTime.UtcNow, DateTime.UtcNow));

        // Act
        await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}