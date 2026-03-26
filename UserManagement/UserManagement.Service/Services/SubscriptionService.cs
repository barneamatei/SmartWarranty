using UserManagement.Domain.DTOs;
using UserManagement.Service.Exceptions;
using UserManagement.Domain.Contracts;
using UserManagement.Domain.Entities;

namespace UserManagement.Service.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionDao _subscriptionDao;
    private readonly IUserDao _userDao;

    public SubscriptionService(ISubscriptionDao subscriptionDao, IUserDao userDao)
    {
        _subscriptionDao = subscriptionDao ?? throw new ArgumentNullException(nameof(subscriptionDao));
        _userDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
    }

    public async Task<SubscriptionResponseDto> CreateAsync(CreateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userDao.GetByIdAsync(dto.UserId, cancellationToken);
        if (user == null)
            throw new DomainException($"User with ID {dto.UserId} not found.");

        var existing = await _subscriptionDao.GetByUserIdAsync(dto.UserId, cancellationToken);
        if (existing != null)
            throw new DomainException($"User {dto.UserId} already has a subscription.");

        var planType = Enum.TryParse<PlanType>(dto.PlanType, true, out var pt) ? pt : PlanType.Free;
        var subscriptionId = Guid.NewGuid();
        var subscription = new Subscription(subscriptionId, dto.UserId, planType, dto.StartDate, dto.EndDate);

        var saved = await _subscriptionDao.AddAsync(subscription, cancellationToken);
        return MapToResponse(saved);
    }

    public async Task<SubscriptionResponseDto?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionDao.GetByIdAsync(subscriptionId, cancellationToken);
        return subscription == null ? null : MapToResponse(subscription);
    }

    public async Task<SubscriptionResponseDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionDao.GetByUserIdAsync(userId, cancellationToken);
        return subscription == null ? null : MapToResponse(subscription);
    }

    public async Task<IEnumerable<SubscriptionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionDao.GetAllAsync(cancellationToken);
        return subscriptions.Select(MapToResponse);
    }

    public async Task<SubscriptionResponseDto> UpdateAsync(Guid subscriptionId, UpdateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionDao.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
            throw new DomainException($"Subscription with ID {subscriptionId} not found.");

        var planType = Enum.TryParse<PlanType>(dto.PlanType, true, out var pt) ? pt : subscription.PlanType;

        if (planType == PlanType.Premium)
            subscription.UpgradeToPremium(dto.EndDate);
        else
            subscription.DowngradeToFree(dto.EndDate);

        var updated = await _subscriptionDao.UpdateAsync(subscription, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _subscriptionDao.DeleteAsync(subscriptionId, cancellationToken);
    }

    private static SubscriptionResponseDto MapToResponse(Subscription subscription)
    {
        return new SubscriptionResponseDto
        {
            SubscriptionId = subscription.SubscriptionId,
            UserId = subscription.UserId,
            PlanType = subscription.PlanType.ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            IsPremium = subscription.IsPremium
        };
    }
}



