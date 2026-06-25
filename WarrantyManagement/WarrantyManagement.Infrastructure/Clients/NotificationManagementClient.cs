using System.Net.Http.Json;
using System.Text.Json;
using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Service.Exceptions;

namespace WarrantyManagement.Infrastructure.Clients;

public class NotificationManagementClient : INotificationManagementClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public NotificationManagementClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task CreateNotificationAsync(Guid userId, string title, string message, string type, string channel, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/notification", new
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Channel = channel,
            Metadata = metadata
        }, cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        throw new DomainException(await ExtractErrorAsync(response, "NotificationManagement", cancellationToken));
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
