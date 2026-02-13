using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Contracts;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Persistence;

namespace UserManagement.Infrastructure.Repositories;

public class FamilyShareRepository : IFamilyShareRepository
{
    private readonly UserManagementDbContext _context;

    public FamilyShareRepository(UserManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<FamilyShare?> GetByIdAsync(Guid shareId, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyShares
            .FirstOrDefaultAsync(f => f.ShareId == shareId, cancellationToken);
    }

    public async Task<IEnumerable<FamilyShare>> GetByOwnerIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyShares
            .Where(f => f.OwnerUserId == ownerUserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FamilyShare>> GetByMemberIdAsync(Guid memberUserId, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyShares
            .Where(f => f.MemberUserId == memberUserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<FamilyShare?> GetByOwnerAndMemberAsync(Guid ownerUserId, Guid memberUserId, CancellationToken cancellationToken = default)
    {
        return await _context.FamilyShares
            .FirstOrDefaultAsync(f => f.OwnerUserId == ownerUserId && f.MemberUserId == memberUserId, cancellationToken);
    }

    public async Task<FamilyShare> AddAsync(FamilyShare familyShare, CancellationToken cancellationToken = default)
    {
        await _context.FamilyShares.AddAsync(familyShare, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return familyShare;
    }

    public async Task<FamilyShare> UpdateAsync(FamilyShare familyShare, CancellationToken cancellationToken = default)
    {
        _context.FamilyShares.Update(familyShare);
        await _context.SaveChangesAsync(cancellationToken);
        return familyShare;
    }

    public async Task<bool> DeleteAsync(Guid shareId, CancellationToken cancellationToken = default)
    {
        var share = await _context.FamilyShares.FindAsync([shareId], cancellationToken);
        if (share == null)
            return false;

        _context.FamilyShares.Remove(share);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
