using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using UserManagement.Application.Auth.Commands;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Settings;
using Xunit;

namespace UserManagement.IntegrationTests;

// Hasher rápido para testes — BCrypt com workFactor:12 é lento demais para testes
internal sealed class PlainTextPasswordHasher : IPasswordHasher
{
    public string Hash(string plain) => plain;
    public bool Verify(string plain, string hash) => plain == hash;
}

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove todos os descritores relacionados ao AppDbContext e ao provider SQL Server.
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    (d.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") ?? false))
                .ToList();

            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("IntegrationTestDb"));

            // Substitui BcryptPasswordHasher pelo PlainTextPasswordHasher no container DI.
            // Necessário para que seed e LoginCommandHandler usem o mesmo algoritmo.
            var hasherDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IPasswordHasher));
            if (hasherDescriptor is not null)
                services.Remove(hasherDescriptor);
            services.AddSingleton<IPasswordHasher, PlainTextPasswordHasher>();
        });

        builder.UseEnvironment("Testing");
    }

    // IAsyncLifetime.InitializeAsync roda depois que o servidor está totalmente iniciado
    // e o pipeline do Program.cs (MapControllers etc.) já foi configurado.
    // Usar builder.Configure() sobrescreveria o pipeline, tirando os controllers do ar (404).
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        context.Database.EnsureCreated();
        await DatabaseSeeder.SeedAsync(context, hasher);
    }

    public new Task DisposeAsync() => base.DisposeAsync().AsTask();

    // Gera um token JWT diretamente com a role desejada, sem passar pelo endpoint de login.
    // Fazer login via HTTP sempre retorna a role do usuário do banco (Administrator),
    // ignorando o parâmetro role e fazendo todos os testes de autorização passarem como 201.
    public HttpClient CreateAuthenticatedClient(string role = "Administrator")
    {
        var client = CreateClient();

        using var scope = Services.CreateScope();
        var jwtSettings = scope.ServiceProvider
            .GetRequiredService<IOptions<JwtSettings>>().Value;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
              new Claim(JwtRegisteredClaimNames.Sub,   Guid.NewGuid().ToString()),
              new Claim(JwtRegisteredClaimNames.Email, $"test-{role.ToLower()}@test.com"),
              new Claim(ClaimTypes.Role,               role),
              new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
          };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        return client;
    }
}