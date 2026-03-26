using Microsoft.EntityFrameworkCore;
using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Domain.Entities;
using WarrantyManagement.Infrastructure.Persistence;

namespace WarrantyManagement.Infrastructure.Repositories;

public class WarrantyRepository : IWarrantyDao
{
    private readonly WarrantyManagementDbContext _context;

    public WarrantyRepository(WarrantyManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Warranty?> GetByIdAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        return await _context.Warranties
            .Include(w => w.Claims)
            .FirstOrDefaultAsync(w => w.WarrantyId == warrantyId, cancellationToken);
    }

    public async Task<IEnumerable<Warranty>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Warranties
            .Include(w => w.Claims)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Warranty>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Warranties
            .Include(w => w.Claims)
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Warranty>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Warranties
            .Include(w => w.Claims)
            .Where(w => w.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Warranty> AddAsync(Warranty warranty, CancellationToken cancellationToken = default)
    {
        await _context.Warranties.AddAsync(warranty, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return warranty;
    }

    public async Task<Warranty> UpdateAsync(Warranty warranty, CancellationToken cancellationToken = default)
    {
        _context.Warranties.Update(warranty);
        await _context.SaveChangesAsync(cancellationToken);
        return warranty;
    }

    public async Task<bool> DeleteAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        var warranty = await _context.Warranties.FindAsync([warrantyId], cancellationToken);
        if (warranty == null)
            return false;

        _context.Warranties.Remove(warranty);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
