namespace UserManagement.Domain.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Invalid credentials.")
        : base(message) { }
}