using MediatR;
using UserManagement.Application.Common.DTOs;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;
using UserManagement.Application.Common.Interfaces;


namespace UserManagement.Application.Auth.Commands;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Mensagem generica: nunca indicar se foi o email ou a senha que falhou
        const string invalidCredentials = "Invalid credentials.";

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !user.IsActive)
            throw new UnauthorizedException(invalidCredentials);

        var passwordValid = _passwordHasher.Verify(request.Password, user.Password.Hash);

        if (!passwordValid)
            throw new UnauthorizedException(invalidCredentials);

        var token = _tokenService.GenerateToken(user);
        var expiresAt = _tokenService.GetExpiration();

        return new AuthResponse(token, expiresAt, user.Role.ToString());
    }
}