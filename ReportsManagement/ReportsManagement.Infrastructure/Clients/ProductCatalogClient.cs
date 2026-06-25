using System.Net.Http.Json;
using System.Text.Json;
using ReportsManagement.Domain.Contracts;
using ReportsManagement.Domain.DTOs;
using ReportsManagement.Infrastructure.Utilities;
using ReportsManagement.Service.Exceptions;

namespace ReportsManagement.Infrastructure.Clients;

public class ProductCatalogClient : IProductCatalogClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyList<ProductReportDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/product", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new DomainException(await ErrorExtractor.ExtractErrorAsync(response, "ProductCatalog", cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<List<ProductApiDto>>(JsonOptions, cancellationToken);
        if (payload == null)
            throw new DomainException("ProductCatalog returned an empty products payload.");

        return payload.Select(product => new ProductReportDto
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Brand = product.Brand,
            Model = product.Model,
            Status = product.Status
        }).ToList();
    }

    private sealed class ProductApiDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
