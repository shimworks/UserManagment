namespace UserManagement.Application.Users.DTOs;
  public sealed record UpdateUserRequest(
      string Name,
      string Email
  );