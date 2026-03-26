using UserManagement.Domain.DTOs;

namespace UserManagement.Service.Services;

public interface ISubscriptionService
{
    Task<SubscriptionResponseDto> CreateAsync(CreateSubscriptionDto dto, CancellationToken cancellationToken = default);

    Task<SubscriptionResponseDto?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    Task<SubscriptionResponseDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<SubscriptionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SubscriptionResponseDto> UpdateAsync(Guid subscriptionId, UpdateSubscriptionDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}

