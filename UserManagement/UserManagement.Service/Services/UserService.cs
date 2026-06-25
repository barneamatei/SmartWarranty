using UserManagement.Domain.DTOs;
using UserManagement.Service.Exceptions;
using UserManagement.Domain.Contracts;
using UserManagement.Domain.Entities;

namespace UserManagement.Service.Services;

public class UserService
{
    private readonly IUserDao _userDao;

    public UserService(IUserDao userDao)
    {
        _userDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new DomainException("Email is required.");

        var existing = await _userDao.GetByEmailAsync(dto.Email, cancellationToken);
        if (existing != null)
            throw new DomainException($"User with email {dto.Email} already exists.");

        var userId = dto.UserId.GetValueOrDefault(Guid.NewGuid());
        var user = new User(userId, dto.Email, UserStatus.Active);
        var profile = new UserProfile(userId, dto.Name, dto.Phone, dto.Language, dto.Preferences);
        user.SetProfile(profile);

        var saved = await _userDao.AddAsync(user, cancellationToken);
        return MapToResponse(saved);
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userDao.GetByIdAsync(userId, cancellationToken);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userDao.GetAllAsync(cancellationToken);
        return users.Select(MapToResponse);
    }

    public async Task<UserResponseDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userDao.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new DomainException($"User with ID {userId} not found.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new DomainException("Email is required.");

        user.UpdateEmail(dto.Email);
        if (user.UserProfile != null)
        {
            user.UserProfile.Update(dto.Name, dto.Phone, dto.Language, dto.Preferences);
        }
        else
        {
            user.SetProfile(new UserProfile(userId, dto.Name, dto.Phone, dto.Language, dto.Preferences));
        }

        var updated = await _userDao.UpdateAsync(user, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userDao.DeleteAsync(userId, cancellationToken);
    }

    private static UserResponseDto MapToResponse(User user)
    {
        return new UserResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            Status = user.Status.ToString(),
            UserProfile = user.UserProfile == null ? null : new UserProfileDto
            {
                UserId = user.UserProfile.UserId,
                Name = user.UserProfile.Name,
                Phone = user.UserProfile.Phone,
                Language = user.UserProfile.Language,
                Preferences = user.UserProfile.Preferences
            },
            Subscription = user.Subscription == null ? null : new SubscriptionResponseDto
            {
                SubscriptionId = user.Subscription.SubscriptionId,
                UserId = user.Subscription.UserId,
                PlanType = user.Subscription.PlanType.ToString(),
                StartDate = user.Subscription.StartDate,
                EndDate = user.Subscription.EndDate,
                IsPremium = user.Subscription.IsPremium
            }
        };
    }
}



