using AutoMapper;
using MediatR;
using UserManagement.Application.Users.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;
using UserManagement.Domain.ValueObjects;

namespace UserManagement.Application.Users.Commands;

public sealed class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserResponse> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (exists)
            throw new ConflictException($"Email '{request.Email}' is already in use.");

        var email = Email.Create(request.Email);
        var hash = _passwordHasher.Hash(request.Password);
        var password = Password.FromHash(hash);
        var user = User.Create(request.Name, email, password, request.Role);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<UserResponse>(user);
    }
}