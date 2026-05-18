using AutoMapper;
using MediatR;
using UserManagement.Application.Users.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.ValueObjects;

namespace UserManagement.Application.Users.Commands;

public sealed class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserResponse> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.Id);

        // Verifica conflito de e-mail apenas se o e-mail mudou
        var emailChanged = !string.Equals(
            user.Email.Value,
            request.Email,
            StringComparison.OrdinalIgnoreCase);

        if (emailChanged)
        {
            var emailInUse = await _userRepository.ExistsByEmailAsync(
                request.Email, cancellationToken);

            if (emailInUse)
                throw new ConflictException($"Email '{request.Email}' is already in use.");

            var newEmail = Email.Create(request.Email);
            user.ChangeEmail(newEmail);
        }

        user.ChangeName(request.Name);

        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<UserResponse>(user);
    }
}