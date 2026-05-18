using UserManagement.Domain.Entities;

public interface ITokenService
{
    string GenerateToken(User user);
    DateTime GetExpiration();
}