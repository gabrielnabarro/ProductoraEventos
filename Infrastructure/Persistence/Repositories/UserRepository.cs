using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _context.User.AnyAsync(user => user.Id == userId, cancellationToken);
    }
}
