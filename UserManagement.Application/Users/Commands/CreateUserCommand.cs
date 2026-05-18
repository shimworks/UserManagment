using MediatR;
using UserManagement.Application.Users.DTOs;
using UserManagement.Domain.Enums;

namespace UserManagement.Application.Users.Commands;

public sealed record CreateUserCommand(
    string Name, string Email, string Password, UserRole Role
) : IRequest<UserResponse>;