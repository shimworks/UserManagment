using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.ValueObjects;
using UserManagement.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(hasher);

        if (await context.Users.AnyAsync()) return;

        var email = Email.Create("admin@userms.com");
        var password = Password.FromHash(hasher.Hash("Admin@1234!"));
        var admin = User.Create("System Administrator", email, password, UserRole.Administrator);

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}