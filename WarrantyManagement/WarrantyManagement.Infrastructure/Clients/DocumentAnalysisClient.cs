using System.Net.Http.Json;
using System.Text.Json;
using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Domain.DTOs;
using WarrantyManagement.Service.Exceptions;

namespace WarrantyManagement.Infrastructure.Clients;

public class DocumentAnalysisClient : IDocumentAnalysisClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public DocumentAnalysisClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<DocumentWarrantyDraftDto> CreateWarrantyDraftAsync(Guid documentId, CreateWarrantyFromAnalyzedDocumentDto request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"api/document/{documentId}/warranty-draft", new
        {
            request.UserId,
            request.ProductId,
            request.DefaultDurationMonths
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ExtractErrorAsync(response, cancellationToken);
            throw new DomainException(errorMessage);
        }

        var draft = await response.Content.ReadFromJsonAsync<DocumentWarrantyDraftDto>(JsonOptions, cancellationToken);
        if (draft == null)
            throw new DomainException("DocumentAnalysis returned an empty warranty draft.");

        return draft;
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

        return $"DocumentAnalysis request failed with status code {(int)response.StatusCode}.";
    }

    private sealed class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
