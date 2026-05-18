using MediatR;
using UserManagement.Application.Common.DTOs;

namespace UserManagement.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<AuthResponse>;