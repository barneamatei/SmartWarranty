using System.Net.Http.Json;
using System.Text.Json;
using ReportsManagement.Domain.Contracts;
using ReportsManagement.Domain.DTOs;
using ReportsManagement.Infrastructure.Utilities;
using ReportsManagement.Service.Exceptions; //nu trebuie sa depinda de servicii

namespace ReportsManagement.Infrastructure.Clients;

public class WarrantyManagementClient : IWarrantyManagementClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public WarrantyManagementClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyList<WarrantyReportDto>> GetWarrantiesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/warranty", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new DomainException(await ErrorExtractor.ExtractErrorAsync(response, "WarrantyManagement", cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<List<WarrantyReportDto>>(JsonOptions, cancellationToken);
        if (payload == null)
            throw new DomainException("WarrantyManagement returned an empty warranties payload.");

        return payload;
    }

    public async Task<IReadOnlyList<WarrantyReportDto>> GetWarrantiesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/warranty/user/{userId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new DomainException(await ErrorExtractor.ExtractErrorAsync(response, "WarrantyManagement", cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<List<WarrantyReportDto>>(JsonOptions, cancellationToken);
        if (payload == null)
            throw new DomainException("WarrantyManagement returned an empty user warranties payload.");

        return payload;
    }

    public async Task<IReadOnlyList<ClaimReportDto>> GetClaimsByWarrantyIdAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/claim/warranty/{warrantyId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new DomainException(await ErrorExtractor.ExtractErrorAsync(response, "WarrantyManagement", cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<List<ClaimReportDto>>(JsonOptions, cancellationToken);
        return payload ?? [];
    }
}
