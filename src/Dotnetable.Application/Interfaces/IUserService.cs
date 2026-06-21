using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default);
    Task<IEnumerable<User>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
    Task<User> CreateAsync(User user, string plainPassword, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, string newPassword, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
