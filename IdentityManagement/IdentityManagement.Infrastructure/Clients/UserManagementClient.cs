using System.Net.Http.Json;
using System.Text.Json;
using IdentityManagement.Domain.Contracts;
using IdentityManagement.Domain.DTOs;
using IdentityManagement.Domain.Exceptions;

namespace IdentityManagement.Infrastructure.Clients;

public class UserManagementClient : IUserManagementClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public UserManagementClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task CreateUserAsync(CreateUserProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/user", request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return;

        throw new DomainException(await ExtractErrorAsync(response, cancellationToken));
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
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

        return $"UserManagement request failed with status code {(int)response.StatusCode}.";
    }

    private sealed class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
