using UserManagement.Domain.ValueObjects;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
namespace UserManagement.Domain.Entities;

public sealed class User : BaseEntity
{
    [Required]
    public string Name { get; private set; }
    [Required]
    public Email Email { get; private set; }
    [Required]
    public Password Password { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    private User() { Name = null!; Email = null!; Password = null!; } // Para o EF Core

    private User(string name, Email email, Password password, UserRole role)
    {
        Name = name;
        Email = email;
        Password = password;
        Role = role;
        IsActive = true;
    }

    public static User Create(string name, Email email, Password password, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        return new User(name, email, password, role);
    }

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Name cannot be empty.");
        Name = newName;
        SetUpdatedAt();
    }

    public void ChangeEmail(Email newEmail)
    {
        Email = newEmail;
        SetUpdatedAt();
    }

    public void ChangePassword(Password newPassword)
    {
        Password = newPassword;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}