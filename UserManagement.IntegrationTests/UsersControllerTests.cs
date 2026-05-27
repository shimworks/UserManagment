using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using UserManagement.Application.Users.DTOs;
using UserManagement.Application.Users.Queries;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class UsersControllerTests
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;

    public UsersControllerTests(CustomWebApplicationFactory factory)
        => _factory = factory;

    public Task InitializeAsync() => _factory.InitializeAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateUser_WithoutToken_ShouldReturn401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            Name = "New User",
            Email = "new@example.com",
            Password = "Pass@1234!",
            Role = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_WithCustomerToken_ShouldReturn403()
    {
        var client = _factory.CreateAuthenticatedClient(role: "Customer");

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            Name = "New User",
            Email = "new@example.com",
            Password = "Pass@1234!",
            Role = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_WithAdminToken_ShouldReturn201WithLocation()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            Name = "New Customer",
            Email = "customer@example.com",
            Password = "Pass@1234!",
            Role = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        body!.Email.Should().Be("customer@example.com");
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetUser_WithNonExistentId_ShouldReturn404()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_ShouldSoftDeleteAndReturn204()
    {
        var client = _factory.CreateAuthenticatedClient();

        // Cria usuario
        var createResponse = await client.PostAsJsonAsync("/api/users", new
        {
            Name = "To Delete",
            Email = "todelete@example.com",
            Password = "Pass@1234!",
            Role = 1
        });

        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();

        // Deleta (soft delete)
        var deleteResponse = await client.DeleteAsync($"/api/users/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Busca e verifica que IsActive = false
        var getResponse = await client.GetAsync($"/api/users/{created.Id}");
        var user = await getResponse.Content.ReadFromJsonAsync<UserResponse>();
        user!.IsActive.Should().BeFalse();
    }
}