using UserManagement.Domain.Enums;

namespace UserManagement.Application.Users.DTOs;
public sealed record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    UserRole Role
);