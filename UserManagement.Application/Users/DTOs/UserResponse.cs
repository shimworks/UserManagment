namespace UserManagement.Application.Users.DTOs;
  public sealed record UserResponse(
      Guid Id,
      string Name,
      string Email,
      string Role,
      bool IsActive,
      DateTime CreatedAt,
      DateTime UpdatedAt
  );
