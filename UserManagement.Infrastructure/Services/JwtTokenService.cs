using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Settings;
using UserManagement.Application.Common.Interfaces;


namespace UserManagement.Infrastructure.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Value;
    }

    public string GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
              new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
              new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
              new Claim(ClaimTypes.Role,               user.Role.ToString()),
              new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
          };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: GetExpiration(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiration()
        => DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
}