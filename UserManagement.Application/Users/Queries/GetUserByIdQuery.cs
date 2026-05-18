using MediatR;
using UserManagement.Application.Users.DTOs;

namespace UserManagement.Application.Users.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserResponse>;
