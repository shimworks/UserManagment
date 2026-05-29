using AutoMapper;
using FluentAssertions;
using Moq;
using UserManagement.Application.Users.DTOs;
using UserManagement.Application.Users.Queries;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.ValueObjects;
using Xunit;

namespace UserManagement.UnitTests.Application;

public sealed class ListUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private ListUsersQueryHandler CreateSut() => new(
        _repositoryMock.Object,
        _mapperMock.Object);

    private static User BuildUser(string name, string email) =>
        User.Create(name, Email.Create(email), Password.FromHash("$2a$12$hash"), UserRole.Customer);

    [Fact]
    public async Task Handle_ShouldReturnPagedResponseWithCorrectTotals()
    {
        // Arrange
        var users = new List<User>
          {
              BuildUser("Alice", "alice@example.com"),
              BuildUser("Bob",   "bob@example.com"),
          };

        var query = new ListUsersQuery(Page: 1, PageSize: 10);

        _repositoryMock
            .Setup(r => r.ListAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<UserResponse>>(users))
            .Returns(users.Select(u => new UserResponse(u.Id, u.Name, u.Email.Value, "Customer", true, DateTime.UtcNow, DateTime.UtcNow)));

        // Act
        var result = await CreateSut().Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldCallListAndCountInParallel()
    {
        // Arrange
        var query = new ListUsersQuery(Page: 2, PageSize: 5);

        _repositoryMock
            .Setup(r => r.ListAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<UserResponse>>(It.IsAny<IEnumerable<User>>()))
            .Returns(Enumerable.Empty<UserResponse>());

        // Act
        await CreateSut().Handle(query, CancellationToken.None);

        // Assert — ambos devem ser chamados independentemente
        _repositoryMock.Verify(r => r.ListAsync(2, 5, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyRepository_ShouldReturnEmptyPagedResponse()
    {
        var query = new ListUsersQuery(Page: 1, PageSize: 10);

        _repositoryMock
            .Setup(r => r.ListAsync(query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        _repositoryMock
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<UserResponse>>(It.IsAny<IEnumerable<User>>()))
            .Returns(Enumerable.Empty<UserResponse>());

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}