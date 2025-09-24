using System.Security.Claims;
using Application.DTOs.Auth;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService,
        IEmailService emailService,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _emailService = emailService;
        _cacheService = cacheService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == request.Email && !u.IsDeleted);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash ?? ""))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Get user roles
        var roles = await GetUserRolesAsync(user.Id);

        // Generate tokens
        var claims = CreateClaims(user, roles);
        var accessToken = _jwtService.GenerateAccessToken(claims);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token hash and reverse lookup in cache (rotation-ready)
        var cacheKey = $"refresh_token:{user.Id}";
        var tokenExpiry = request.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromDays(7);
        var refreshHash = HashToken(refreshToken);
        await _cacheService.SetAsync(cacheKey, refreshHash, tokenExpiry);
        await _cacheService.SetAsync($"refresh_lookup:{refreshHash}", user.Id.ToString(), tokenExpiry);

        // Update user's last login
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(accessToken),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.Users.GetAsync(u => u.Email == request.Email || u.UserName == request.UserName);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email or username already exists");
        }

        // Validate password strength
        if (!_passwordService.IsPasswordStrong(request.Password))
        {
            throw new ArgumentException("Password does not meet security requirements");
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordService.HashPassword(request.Password),
            EmailConfirmed = false,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign default role (Customer)
        var customerRole = await _unitOfWork.Roles.GetAsync(r => r.Name == "Customer");
        if (customerRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = customerRole.Id
            };
            await _unitOfWork.UserRoles.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        // Send email confirmation
        await SendEmailConfirmationAsync(user);

        // Generate tokens
        var roles = new List<string> { "Customer" };
        var claims = CreateClaims(user, roles);
        var accessToken = _jwtService.GenerateAccessToken(claims);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token hash and reverse lookup in cache
        var cacheKey = $"refresh_token:{user.Id}";
        var refreshHash = HashToken(refreshToken);
        await _cacheService.SetAsync(cacheKey, refreshHash, TimeSpan.FromDays(7));
        await _cacheService.SetAsync($"refresh_lookup:{refreshHash}", user.Id.ToString(), TimeSpan.FromDays(7));

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(accessToken),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Validate access token
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Invalid access token");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token claims");
        }

        // Verify refresh token
        var cacheKey = $"refresh_token:{userId}";
        var storedRefreshHash = await _cacheService.GetAsync<string>(cacheKey);
        var requestHash = HashToken(request.RefreshToken);

        // Detect reuse: if provided token was already used, revoke family
        var reused = await _cacheService.GetAsync<string>($"used_refresh:{requestHash}");
        if (!string.IsNullOrEmpty(reused))
        {
            // Revoke the whole family
            await _cacheService.RemoveAsync(cacheKey);
            throw new UnauthorizedAccessException("Refresh token reuse detected");
        }

        if (storedRefreshHash != requestHash)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Get user
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId && !u.IsDeleted);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Get user roles
        var roles = await GetUserRolesAsync(user.Id);

        // Generate new tokens
        var claims = CreateClaims(user, roles);
        var newAccessToken = _jwtService.GenerateAccessToken(claims);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Mark old token as used to prevent reuse
        await _cacheService.SetAsync($"used_refresh:{requestHash}", "1", TimeSpan.FromDays(7));
        // Remove reverse lookup of old token
        await _cacheService.RemoveAsync($"refresh_lookup:{requestHash}");

        // Store new hash and reverse lookup
        var newHash = HashToken(newRefreshToken);
        await _cacheService.SetAsync(cacheKey, newHash, TimeSpan.FromDays(7));
        await _cacheService.SetAsync($"refresh_lookup:{newHash}", user.Id.ToString(), TimeSpan.FromDays(7));

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(newAccessToken),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles
            }
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        // Find user by refresh token (reverse lookup)
        var hash = HashToken(refreshToken);
        var userIdString = await _cacheService.GetAsync<string>($"refresh_lookup:{hash}");
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var parsed))
        {
            userId = parsed;
        }
        if (userId == null)
        {
            return false;
        }

        // Remove refresh token from cache and reverse lookup; mark token as used
        var cacheKey = $"refresh_token:{userId}";
        await _cacheService.RemoveAsync(cacheKey);
        await _cacheService.RemoveAsync($"refresh_lookup:{hash}");
        await _cacheService.SetAsync($"used_refresh:{hash}", "1", TimeSpan.FromDays(7));

        return true;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        return await LogoutAsync(refreshToken);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        return Task.FromResult(_jwtService.ValidateToken(token));
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == email && !u.IsDeleted);
        return user?.Id;
    }

    private async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == userId);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetAllAsync(r => roleIds.Contains(r.Id) && !r.IsDeleted);
        return roles.Select(r => r.Name).ToList();
    }

    private static List<Claim> CreateClaims(User user, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("emailConfirmed", user.EmailConfirmed.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private async Task<Guid?> FindUserIdByRefreshTokenAsync(string refreshToken)
    {
        var hash = HashToken(refreshToken);
        var userIdString = await _cacheService.GetAsync<string>($"refresh_lookup:{hash}");
        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static string HashToken(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private async Task SendEmailConfirmationAsync(User user)
    {
        // TODO: Implement email confirmation
        await Task.CompletedTask;
    }
}
