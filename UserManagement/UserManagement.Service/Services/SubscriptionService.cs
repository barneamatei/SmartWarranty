using UserManagement.Service.DTOs;
using UserManagement.Service.Exceptions;
using UserManagement.Domain.Contracts;
using UserManagement.Domain.Entities;

namespace UserManagement.Service.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, IUserRepository userRepository)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<SubscriptionResponseDto> CreateAsync(CreateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId, cancellationToken);
        if (user == null)
            throw new DomainException($"User with ID {dto.UserId} not found.");

        var existing = await _subscriptionRepository.GetByUserIdAsync(dto.UserId, cancellationToken);
        if (existing != null)
            throw new DomainException($"User {dto.UserId} already has a subscription.");

        var planType = Enum.TryParse<PlanType>(dto.PlanType, true, out var pt) ? pt : PlanType.Free;
        var subscriptionId = Guid.NewGuid();
        var subscription = new Subscription(subscriptionId, dto.UserId, planType, dto.StartDate, dto.EndDate);

        var saved = await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        return MapToResponse(saved);
    }

    public async Task<SubscriptionResponseDto?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
        return subscription == null ? null : MapToResponse(subscription);
    }

    public async Task<SubscriptionResponseDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        return subscription == null ? null : MapToResponse(subscription);
    }

    public async Task<IEnumerable<SubscriptionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync(cancellationToken);
        return subscriptions.Select(MapToResponse);
    }

    public async Task<SubscriptionResponseDto> UpdateAsync(Guid subscriptionId, UpdateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
        if (subscription == null)
            throw new DomainException($"Subscription with ID {subscriptionId} not found.");

        var planType = Enum.TryParse<PlanType>(dto.PlanType, true, out var pt) ? pt : subscription.PlanType;

        if (planType == PlanType.Premium)
            subscription.UpgradeToPremium(dto.EndDate);
        else
            subscription.DowngradeToFree(dto.EndDate);

        var updated = await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _subscriptionRepository.DeleteAsync(subscriptionId, cancellationToken);
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
