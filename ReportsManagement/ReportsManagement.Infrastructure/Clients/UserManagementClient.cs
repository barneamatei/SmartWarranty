using System.Net.Http.Json;
using System.Text.Json;
using ReportsManagement.Domain.Contracts;
using ReportsManagement.Domain.DTOs;
using ReportsManagement.Infrastructure.Utilities;
using ReportsManagement.Service.Exceptions;

namespace ReportsManagement.Infrastructure.Clients;

public class UserManagementClient : IUserManagementClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public UserManagementClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyList<UserReportDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/user", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new DomainException(await ErrorExtractor.ExtractErrorAsync(response, "UserManagement", cancellationToken));

        var payload = await response.Content.ReadFromJsonAsync<List<UserApiDto>>(JsonOptions, cancellationToken);
        if (payload == null)
            throw new DomainException("UserManagement returned an empty users payload.");

        return payload.Select(user => new UserReportDto
        {
            UserId = user.UserId,
            Email = user.Email,
            Status = user.Status,
            Name = user.UserProfile?.Name ?? user.Email,
            Phone = user.UserProfile?.Phone,
            Language = user.UserProfile?.Language,
            SubscriptionPlan = user.Subscription?.PlanType,
            SubscriptionEndDate = user.Subscription?.EndDate,
            IsPremium = user.Subscription?.IsPremium ?? false
        }).ToList();
    }

    private sealed class UserApiDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public UserProfileApiDto? UserProfile { get; set; }
        public SubscriptionApiDto? Subscription { get; set; }
    }

    private sealed class UserProfileApiDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Language { get; set; }
    }

    private sealed class SubscriptionApiDto
    {
        public string PlanType { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public bool IsPremium { get; set; }
    }
}
