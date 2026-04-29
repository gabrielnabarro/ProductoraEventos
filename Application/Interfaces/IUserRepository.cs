using Domain.Entities;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
