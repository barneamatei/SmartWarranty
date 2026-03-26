using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Contracts;

public interface IUserDao
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}

