namespace Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default);
}
