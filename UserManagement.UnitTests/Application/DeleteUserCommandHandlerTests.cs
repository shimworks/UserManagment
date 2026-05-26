using FluentAssertions;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Users.Commands;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.ValueObjects;
using Xunit;

namespace UserManagement.UnitTests.Application;

public sealed class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private DeleteUserCommandHandler CreateSut() => new(
        _repositoryMock.Object,
        _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_WithExistingUser_ShouldDeactivateAndNotPhysicallyDelete()
    {
        // Arrange
        var user = User.Create(
            "John",
            Email.Create("john@example.com"),
            Password.FromHash("$2a$12$hash"),
            UserRole.Customer);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await CreateSut().Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        // Assert — soft delete: IsActive deve ser false
        user.IsActive.Should().BeFalse();

        // Update deve ter sido chamado (persiste o soft delete)
        _repositoryMock.Verify(r => r.Update(user), Times.Once);

        // Delete fisico NUNCA deve ser chamado
        _repositoryMock.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);

        // Commit deve ter sido chamado uma vez
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await CreateSut().Handle(new DeleteUserCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}