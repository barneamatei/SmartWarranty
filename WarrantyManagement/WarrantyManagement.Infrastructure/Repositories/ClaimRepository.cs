using Microsoft.EntityFrameworkCore;
using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Domain.Entities;
using WarrantyManagement.Infrastructure.Persistence;

namespace WarrantyManagement.Infrastructure.Repositories;

public class ClaimRepository : IClaimDao
{
    private readonly WarrantyManagementDbContext _context;

    public ClaimRepository(WarrantyManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .FirstOrDefaultAsync(c => c.ClaimId == claimId, cancellationToken);
    }

    public async Task<IEnumerable<Claim>> GetByWarrantyIdAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Where(c => c.WarrantyId == warrantyId)
            .OrderByDescending(c => c.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Claim> AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await _context.Claims.AddAsync(claim, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return claim;
    }

    public async Task<Claim> UpdateAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        _context.Claims.Update(claim);
        await _context.SaveChangesAsync(cancellationToken);
        return claim;
    }
}
