using System.Net.Http.Json;
using System.Text.Json;
using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Service.Exceptions;

namespace WarrantyManagement.Infrastructure.Clients;

public class UserManagementClient : IUserManagementClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public UserManagementClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/user/{userId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;
        if (response.IsSuccessStatusCode)
            return true;

        throw new DomainException(await ExtractErrorAsync(response, "UserManagement", cancellationToken));
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response, string serviceName, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(payload?.Error))
                return payload.Error;
        }
        catch
        {
        }

        return $"{serviceName} request failed with status code {(int)response.StatusCode}.";
    }

    private sealed class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
