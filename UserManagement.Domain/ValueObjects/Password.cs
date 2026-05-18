namespace UserManagement.Domain.ValueObjects;
using UserManagement.Domain.Exceptions;

public sealed class Password
{
    public string Hash { get; }

    private Password(string hash) => Hash = hash;

    public static Password FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty.");
        return new Password(hash);
    }

    public override string ToString() => "[PROTECTED]";
    public override bool Equals(object? obj) => obj is Password p && Hash == p.Hash;
    public override int GetHashCode() => Hash.GetHashCode();
}