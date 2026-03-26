using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Contracts;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Persistence;

namespace UserManagement.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionDao
{
    private readonly UserManagementDbContext _context;

    public SubscriptionRepository(UserManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);
    }

    public async Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<bool> DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.FindAsync([subscriptionId], cancellationToken);
        if (subscription == null)
            return false;

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

