using UserManagement.Service.DTOs;

namespace UserManagement.Service.Services;

public interface IUserService
{
    Task<UserResponseDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    Task<UserResponseDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserResponseDto> UpdateAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
