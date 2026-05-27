using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using UserManagement.Application.Auth.Commands;
using UserManagement.Application.Common.DTOs;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@userms.com",
            Password = "Admin@1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.Role.Should().Be("Administrator");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@userms.com",
            Password = "WrongPassword"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_ShouldReturn422()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "not-an-email",
            Password = "Pass@1234"
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}