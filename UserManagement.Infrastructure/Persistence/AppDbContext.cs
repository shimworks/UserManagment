using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await SaveChangesAsync(cancellationToken);
}