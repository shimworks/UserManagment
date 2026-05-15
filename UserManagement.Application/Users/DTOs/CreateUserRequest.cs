using UserManagement.Domain.Enums;

public sealed record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    UserRole Role
);