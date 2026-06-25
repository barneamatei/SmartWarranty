using ReportsManagement.Domain.DTOs;

namespace ReportsManagement.Domain.Contracts;

public interface IWarrantyManagementClient
{
    Task<IReadOnlyList<WarrantyReportDto>> GetWarrantiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WarrantyReportDto>> GetWarrantiesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClaimReportDto>> GetClaimsByWarrantyIdAsync(Guid warrantyId, CancellationToken cancellationToken = default);
}
