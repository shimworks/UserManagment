using MediatR;
using UserManagement.Application.Common.DTOs;
using UserManagement.Application.Users.DTOs;

namespace UserManagement.Application.Users.Queries;

public sealed record ListUsersQuery(int Page = 1, int PageSize = 10)
    : IRequest<PagedResponse<UserResponse>>;
