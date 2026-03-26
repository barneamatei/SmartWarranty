using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Contracts;

public interface ISubscriptionDao
{
    Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

    Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}

