using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);

    public async Task<IEnumerable<User>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await _context.Users
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await _context.Users.CountAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user) => _context.Users.Update(user);

    public void Delete(User user) => _context.Users.Remove(user);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
}