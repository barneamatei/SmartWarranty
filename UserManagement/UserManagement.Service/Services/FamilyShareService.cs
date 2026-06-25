using UserManagement.Domain.DTOs;
using UserManagement.Service.Exceptions;
using UserManagement.Domain.Contracts;
using UserManagement.Domain.Entities;

namespace UserManagement.Service.Services;

public class FamilyShareService
{
    private readonly IFamilyShareDao _familyShareDao;
    private readonly IUserDao _userDao;

    public FamilyShareService(IFamilyShareDao familyShareDao, IUserDao userDao)
    {
        _familyShareDao = familyShareDao ?? throw new ArgumentNullException(nameof(familyShareDao));
        _userDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
    }

    public async Task<FamilyShareResponseDto> CreateAsync(CreateFamilyShareDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.OwnerUserId == dto.MemberUserId)
            throw new DomainException("A user cannot share with themselves.");

        var existing = await _familyShareDao.GetByOwnerAndMemberAsync(dto.OwnerUserId, dto.MemberUserId, cancellationToken);
        if (existing != null)
            throw new FamilyShareAlreadyExistsException(dto.OwnerUserId, dto.MemberUserId);

        var owner = await _userDao.GetByIdAsync(dto.OwnerUserId, cancellationToken);
        if (owner == null)
            throw new DomainException($"Owner user with ID {dto.OwnerUserId} not found.");

        if (owner.Status != UserStatus.Active)
            throw new DomainException("Only active users can be added to FamilyShare as owner.");

        var member = await _userDao.GetByIdAsync(dto.MemberUserId, cancellationToken);
        if (member == null)
            throw new DomainException($"Member user with ID {dto.MemberUserId} not found.");

        if (member.Status != UserStatus.Active)
            throw new DomainException("Only active users can be added to FamilyShare as member.");

        var shareId = Guid.NewGuid();
        var permissions = (Permissions)dto.Permissions;
        var familyShare = new FamilyShare(shareId, dto.OwnerUserId, dto.MemberUserId, permissions);

        var saved = await _familyShareDao.AddAsync(familyShare, cancellationToken);
        return MapToResponse(saved);
    }

    public async Task<FamilyShareResponseDto?> GetByIdAsync(Guid shareId, CancellationToken cancellationToken = default)
    {
        var share = await _familyShareDao.GetByIdAsync(shareId, cancellationToken);
        return share == null ? null : MapToResponse(share);
    }

    public async Task<IEnumerable<FamilyShareResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var shares = await _familyShareDao.GetAllAsync(cancellationToken);
        return shares.Select(MapToResponse);
    }

    public async Task<IEnumerable<FamilyShareResponseDto>> GetByOwnerIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        var shares = await _familyShareDao.GetByOwnerIdAsync(ownerUserId, cancellationToken);
        return shares.Select(MapToResponse);
    }

    public async Task<IEnumerable<FamilyShareResponseDto>> GetByMemberIdAsync(Guid memberUserId, CancellationToken cancellationToken = default)
    {
        var shares = await _familyShareDao.GetByMemberIdAsync(memberUserId, cancellationToken);
        return shares.Select(MapToResponse);
    }

    public async Task<FamilyShareResponseDto> UpdateAsync(Guid shareId, UpdateFamilyShareDto dto, CancellationToken cancellationToken = default)
    {
        var share = await _familyShareDao.GetByIdAsync(shareId, cancellationToken);
        if (share == null)
            throw new DomainException($"Family share with ID {shareId} not found.");

        share.UpdatePermissions((Permissions)dto.Permissions);
        var updated = await _familyShareDao.UpdateAsync(share, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid shareId, CancellationToken cancellationToken = default)
    {
        return await _familyShareDao.DeleteAsync(shareId, cancellationToken);
    }

    private static FamilyShareResponseDto MapToResponse(FamilyShare share)
    {
        return new FamilyShareResponseDto
        {
            ShareId = share.ShareId,
            OwnerUserId = share.OwnerUserId,
            MemberUserId = share.MemberUserId,
            Permissions = (int)share.Permissions
        };
    }
}



