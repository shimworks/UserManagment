using AutoMapper;
using MediatR;
using UserManagement.Application.Common.DTOs;
using UserManagement.Application.Users.DTOs;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Application.Users.Queries;

public sealed class ListUsersQueryHandler
    : IRequestHandler<ListUsersQuery, PagedResponse<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public ListUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<UserResponse>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Executa contagem e listagem em paralelo para melhor performance
        var usersTask = _userRepository.ListAsync(request.Page, request.PageSize, cancellationToken);
        var countTask = _userRepository.CountAsync(cancellationToken);

        await Task.WhenAll(usersTask, countTask);

        var items = _mapper.Map<IEnumerable<UserResponse>>(await usersTask);
        var total = await countTask;

        return new PagedResponse<UserResponse>(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: total);
    }
}