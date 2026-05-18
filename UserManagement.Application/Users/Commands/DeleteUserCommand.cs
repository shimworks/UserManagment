using MediatR;

namespace UserManagement.Application.Users.Commands;

public sealed record DeleteUserCommand(Guid Id) : IRequest<Unit>;
