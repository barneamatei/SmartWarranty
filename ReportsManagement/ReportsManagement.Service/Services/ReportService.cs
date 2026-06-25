using ReportsManagement.Domain.Contracts;
using ReportsManagement.Domain.DTOs;
using ReportsManagement.Service.Exceptions;

namespace ReportsManagement.Service.Services;

public class ReportService
{
    private readonly IUserManagementClient _userManagementClient;
    private readonly IWarrantyManagementClient _warrantyManagementClient;
    private readonly IProductCatalogClient _productCatalogClient;
    private readonly IReadOnlyDictionary<string, IReportExporter> _exporters;

    public ReportService(
        IUserManagementClient userManagementClient,
        IWarrantyManagementClient warrantyManagementClient,
        IProductCatalogClient productCatalogClient,
        IEnumerable<IReportExporter> exporters)
    {
        _userManagementClient = userManagementClient ?? throw new ArgumentNullException(nameof(userManagementClient));
        _warrantyManagementClient = warrantyManagementClient ?? throw new ArgumentNullException(nameof(warrantyManagementClient));
        _productCatalogClient = productCatalogClient ?? throw new ArgumentNullException(nameof(productCatalogClient));
        _exporters = exporters?.ToDictionary(exporter => exporter.Format, StringComparer.OrdinalIgnoreCase)
            ?? throw new ArgumentNullException(nameof(exporters));
    }

    public async Task<ReportPreviewDto> GetPortfolioPreviewAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await BuildPortfolioReportAsync(userId, cancellationToken);
        return ToPreview(report);
    }

    public async Task<ReportFileDto> ExportPortfolioAsync(string format, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await BuildPortfolioReportAsync(userId, cancellationToken);
        return Export(report, format);
    }

    public async Task<ReportPreviewDto> GetExpiringWarrantiesPreviewAsync(int daysAhead, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await BuildExpiringWarrantiesReportAsync(daysAhead, userId, cancellationToken);
        return ToPreview(report);
    }

    public async Task<ReportFileDto> ExportExpiringWarrantiesAsync(string format, int daysAhead, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await BuildExpiringWarrantiesReportAsync(daysAhead, userId, cancellationToken);
        return Export(report, format);
    }

    private async Task<ReportDefinition> BuildPortfolioReportAsync(Guid? userId, CancellationToken cancellationToken)
    {
        var generatedAt = DateTime.UtcNow;
        var usersTask = _userManagementClient.GetUsersAsync(cancellationToken);
        var warrantiesTask = userId.HasValue
            ? _warrantyManagementClient.GetWarrantiesByUserIdAsync(userId.Value, cancellationToken)
            : _warrantyManagementClient.GetWarrantiesAsync(cancellationToken);
        var productsTask = _productCatalogClient.GetProductsAsync(cancellationToken);

        await Task.WhenAll(usersTask, warrantiesTask, productsTask);

        var users = userId.HasValue
            ? usersTask.Result.Where(user => user.UserId == userId.Value).ToList()
            : usersTask.Result;
        var warranties = warrantiesTask.Result;
        var warrantyProductIds = warranties.Select(warranty => warranty.ProductId).ToHashSet();
        var products = productsTask.Result.Where(product => warrantyProductIds.Contains(product.ProductId)).ToList();

        var claimsByWarrantyId = await LoadClaimsByWarrantyIdAsync(warranties, cancellationToken);
        var userMap = users.ToDictionary(user => user.UserId);
        var productMap = products.ToDictionary(product => product.ProductId);

        var rows = warranties
            .OrderBy(warranty => warranty.ExpiryDate)
            .Select(warranty =>
            {
                userMap.TryGetValue(warranty.UserId, out var user);
                productMap.TryGetValue(warranty.ProductId, out var product);
                claimsByWarrantyId.TryGetValue(warranty.WarrantyId, out var claims);
                var claimList = claims ?? [];
                var openClaims = claimList.Count(claim => !claim.ClosedAt.HasValue);
                var lastClaimDate = claimList.OrderByDescending(claim => claim.OpenedAt).FirstOrDefault()?.OpenedAt;

                return (IReadOnlyList<string>)
                [
                    user?.Name ?? "Unknown user",
                    user?.Email ?? "-",
                    user?.SubscriptionPlan ?? "No plan",
                    user?.IsPremium == true ? "Yes" : "No",
                    product is null ? "Unknown product" : $"{product.Brand} {product.Name} {product.Model}".Trim(),
                    warranty.Status,
                    warranty.PurchaseDate.ToString("yyyy-MM-dd"),
                    warranty.ExpiryDate.ToString("yyyy-MM-dd"),
                    claimList.Count.ToString(),
                    openClaims.ToString(),
                    lastClaimDate?.ToString("yyyy-MM-dd") ?? "-"
                ];
            })
            .ToList();

        return new ReportDefinition
        {
            ReportType = "portfolio",
            Title = userId.HasValue ? "My Warranty Portfolio" : "Customer Warranty Portfolio",
            Subtitle = userId.HasValue ? "Warranty portfolio for the selected account" : "Warranty portfolio for all accounts",
            FileNameStem = userId.HasValue ? $"my-portfolio-report-{generatedAt:yyyyMMddHHmmss}" : $"portfolio-report-{generatedAt:yyyyMMddHHmmss}",
            GeneratedAtUtc = generatedAt,
            Columns =
            [
                "Customer",
                "Email",
                "Plan",
                "Premium",
                "Product",
                "Warranty Status",
                "Purchase Date",
                "Expiry Date",
                "Claims",
                "Open Claims",
                "Last Claim"
            ],
            Rows = rows,
            Summary = new Dictionary<string, string>
            {
                ["Users"] = users.Count.ToString(),
                ["Products"] = products.Count.ToString(),
                ["Warranties"] = warranties.Count.ToString(),
                ["Total Claims"] = claimsByWarrantyId.Values.Sum(list => list.Count).ToString(),
                ["Expired Warranties"] = warranties.Count(w => w.ExpiryDate.Date < DateTime.UtcNow.Date).ToString()
            }
        };
    }

    private async Task<ReportDefinition> BuildExpiringWarrantiesReportAsync(int daysAhead, Guid? userId, CancellationToken cancellationToken)
    {
        if (daysAhead <= 0)
            throw new DomainException("daysAhead must be greater than 0.");

        var generatedAt = DateTime.UtcNow;
        var cutoffDate = generatedAt.Date.AddDays(daysAhead);

        var usersTask = _userManagementClient.GetUsersAsync(cancellationToken);
        var warrantiesTask = userId.HasValue
            ? _warrantyManagementClient.GetWarrantiesByUserIdAsync(userId.Value, cancellationToken)
            : _warrantyManagementClient.GetWarrantiesAsync(cancellationToken);
        var productsTask = _productCatalogClient.GetProductsAsync(cancellationToken);

        await Task.WhenAll(usersTask, warrantiesTask, productsTask);

        var users = usersTask.Result.ToDictionary(user => user.UserId);
        var filteredWarranties = warrantiesTask.Result
            .Where(warranty => warranty.ExpiryDate.Date >= generatedAt.Date && warranty.ExpiryDate.Date <= cutoffDate)
            .OrderBy(warranty => warranty.ExpiryDate)
            .ToList();
        var warrantyProductIds = filteredWarranties.Select(warranty => warranty.ProductId).ToHashSet();
        var products = productsTask.Result
            .Where(product => warrantyProductIds.Contains(product.ProductId))
            .ToDictionary(product => product.ProductId);

        var rows = filteredWarranties
            .Select(warranty =>
            {
                users.TryGetValue(warranty.UserId, out var user);
                products.TryGetValue(warranty.ProductId, out var product);
                var daysRemaining = (warranty.ExpiryDate.Date - generatedAt.Date).Days;

                return (IReadOnlyList<string>)
                [
                    warranty.WarrantyId.ToString(),
                    user?.Name ?? "Unknown user",
                    user?.Email ?? "-",
                    product is null ? "Unknown product" : $"{product.Brand} {product.Name} {product.Model}".Trim(),
                    warranty.Status,
                    warranty.ExpiryDate.ToString("yyyy-MM-dd"),
                    daysRemaining.ToString(),
                    user?.SubscriptionPlan ?? "No plan"
                ];
            })
            .ToList();

        return new ReportDefinition
        {
            ReportType = "expiring-warranties",
            Title = userId.HasValue ? "My Warranties Expiring Soon" : "Warranties Expiring Soon",
            Subtitle = $"Warranties expiring in the next {daysAhead} days",
            FileNameStem = userId.HasValue ? $"my-expiring-warranties-{daysAhead}d-{generatedAt:yyyyMMddHHmmss}" : $"expiring-warranties-{daysAhead}d-{generatedAt:yyyyMMddHHmmss}",
            GeneratedAtUtc = generatedAt,
            Columns =
            [
                "Warranty Id",
                "Customer",
                "Email",
                "Product",
                "Warranty Status",
                "Expiry Date",
                "Days Remaining",
                "Plan"
            ],
            Rows = rows,
            Summary = new Dictionary<string, string>
            {
                ["Window (days)"] = daysAhead.ToString(),
                ["Matching Warranties"] = filteredWarranties.Count.ToString(),
                ["Premium Customers"] = filteredWarranties.Count(warranty => users.TryGetValue(warranty.UserId, out var user) && user.IsPremium).ToString()
            }
        };
    }

    private async Task<Dictionary<Guid, IReadOnlyList<ClaimReportDto>>> LoadClaimsByWarrantyIdAsync(
        IReadOnlyList<WarrantyReportDto> warranties,
        CancellationToken cancellationToken)
    {
        var tasks = warranties.Select(async warranty => new
        {
            warranty.WarrantyId,
            Claims = await _warrantyManagementClient.GetClaimsByWarrantyIdAsync(warranty.WarrantyId, cancellationToken)
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(item => item.WarrantyId, item => item.Claims);
    }

    private ReportFileDto Export(ReportDefinition report, string format)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
            throw new DomainException($"Unsupported export format '{format}'. Supported formats: {string.Join(", ", _exporters.Keys.OrderBy(key => key))}.");

        return new ReportFileDto
        {
            FileName = $"{report.FileNameStem}.{exporter.Format.ToLowerInvariant()}",
            ContentType = exporter.ContentType,
            Content = exporter.Export(report)
        };
    }

    private static ReportPreviewDto ToPreview(ReportDefinition report)
    {
        var rows = report.Rows
            .Select(row => (IReadOnlyDictionary<string, string>)report.Columns
                .Zip(row, (column, value) => new KeyValuePair<string, string>(column, value))
                .ToDictionary(pair => pair.Key, pair => pair.Value))
            .ToList();

        return new ReportPreviewDto
        {
            ReportType = report.ReportType,
            Title = report.Title,
            Subtitle = report.Subtitle,
            GeneratedAtUtc = report.GeneratedAtUtc,
            RecordCount = report.Rows.Count,
            Columns = report.Columns,
            Rows = rows,
            Summary = report.Summary
        };
    }
}
