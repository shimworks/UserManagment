using System.Text.RegularExpressions;
using UserManagement.Domain.Exceptions;

namespace UserManagement.Domain.ValueObjects;

public sealed class Email
{
    private static readonly Regex _regex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");
        if (!_regex.IsMatch(value))
            throw new DomainException($"'{value}' is not a valid email.");
        return new Email(value.ToLowerInvariant());
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Email e && Value == e.Value;
    public override int GetHashCode() => Value.GetHashCode();
}