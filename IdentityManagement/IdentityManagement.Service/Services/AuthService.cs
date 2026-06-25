using System.Security.Claims;
using IdentityManagement.Domain.Configuration;
using IdentityManagement.Domain.Contracts;
using IdentityManagement.Domain.DTOs;
using IdentityManagement.Domain.Entities;
using IdentityManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityManagement.Service.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenDao _refreshTokenDao;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserManagementClient _userManagementClient;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IRefreshTokenDao refreshTokenDao,
        IJwtTokenGenerator jwtTokenGenerator,
        IUserManagementClient userManagementClient,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _refreshTokenDao = refreshTokenDao ?? throw new ArgumentNullException(nameof(refreshTokenDao));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _userManagementClient = userManagementClient ?? throw new ArgumentNullException(nameof(userManagementClient));
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new DomainException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            throw new DomainException(string.Join(" ", createResult.Errors.Select(x => x.Description)));

        await _userManager.AddToRoleAsync(user, "User");

        try
        {
            await _userManagementClient.CreateUserAsync(new CreateUserProfileRequestDto
            {
                UserId = user.Id,
                Email = user.Email ?? dto.Email,
                Name = $"{user.FirstName} {user.LastName}".Trim()
            }, cancellationToken);
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            throw new DomainException("Invalid credentials.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            throw new DomainException("Invalid credentials.");

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenDao.GetByTokenAsync(dto.RefreshToken, cancellationToken);
        if (storedToken == null || !storedToken.IsActive)
            throw new DomainException("Refresh token is invalid or expired.");

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = _jwtTokenGenerator.GenerateRefreshToken();
        await _refreshTokenDao.UpdateAsync(storedToken, cancellationToken);

        var replacementToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = storedToken.UserId,
            Token = storedToken.ReplacedByToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        };

        await _refreshTokenDao.AddAsync(replacementToken, cancellationToken);

        var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(storedToken.User, cancellationToken);
        var profile = await MapProfileAsync(storedToken.User);

        return new AuthResponseDto
        {
            AccessToken = accessToken.AccessToken,
            RefreshToken = replacementToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            User = profile
        };
    }

    public async Task LogoutAsync(LogoutRequestDto dto, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenDao.GetByTokenAsync(dto.RefreshToken, cancellationToken);
        if (storedToken == null)
            return;

        if (!storedToken.IsRevoked)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenDao.UpdateAsync(storedToken, cancellationToken);
        }
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new DomainException("Authenticated user is invalid.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new DomainException("Authenticated user not found.");

        return await MapProfileAsync(user);
    }

    public async Task ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequestDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new DomainException("New password confirmation does not match.");

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new DomainException("Authenticated user is invalid.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
            throw new DomainException("Authenticated user not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new DomainException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user, cancellationToken);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        };

        await _refreshTokenDao.AddAsync(refreshToken, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken.AccessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            User = await MapProfileAsync(user)
        };
    }

    private async Task<UserProfileDto> MapProfileAsync(ApplicationUser user)
    {
        return new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = await _userManager.GetRolesAsync(user)
        };
    }
}
