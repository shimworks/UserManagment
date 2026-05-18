namespace UserManagement.Application.Common.DTOs;

public sealed record AuthResponse(string Token, DateTime ExpiresAt, string Role);
