using System.Net.Http.Json;
using System.Text.Json;

namespace ReportsManagement.Infrastructure.Utilities;

internal static class ErrorExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<string> ExtractErrorAsync(HttpResponseMessage response, string serviceName, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(payload?.Error))
                return payload.Error!;
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
