using MediatR;
using UserManagement.Application.Users.DTOs;

namespace UserManagement.Application.Users.Commands;

public sealed record UpdateUserCommand(
    Guid Id, string Name, string Email
) : IRequest<UserResponse>;