using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}