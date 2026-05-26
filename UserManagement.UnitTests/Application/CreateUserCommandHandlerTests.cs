using AutoMapper;
using FluentAssertions;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Users.Commands;
using UserManagement.Application.Users.DTOs;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.Entities;
using Xunit;

namespace UserManagement.UnitTests.Application;

public sealed class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private CreateUserCommandHandler CreateSut() => new(
        _repositoryMock.Object,
        _hasherMock.Object,
        _unitOfWorkMock.Object,
        _mapperMock.Object);

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUserAndCommit()
    {
        // Arrange
        var command = new CreateUserCommand("John", "john@example.com", "Pass@1234", UserRole.Customer);

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _hasherMock
            .Setup(h => h.Hash(command.Password))
            .Returns("$2a$12$hashedpassword");

        _mapperMock
            .Setup(m => m.Map<UserResponse>(It.IsAny<User>()))
            .Returns(new UserResponse(Guid.NewGuid(), command.Name, command.Email, "Customer", true, DateTime.UtcNow, DateTime.UtcNow));

        // Act
        var result = await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateUserCommand("John", "john@example.com", "Pass@1234", UserRole.Customer);

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await CreateSut().Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}