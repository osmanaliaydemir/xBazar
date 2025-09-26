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
        var jwtId = _jwtService.GetJwtId(accessToken);

        // Revoke all existing refresh tokens for this user
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, "New login");

        // Store refresh token in database
        var tokenExpiry = request.RememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromDays(7);
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            JwtId = jwtId ?? Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.Add(tokenExpiry),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);

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
        var jwtId = _jwtService.GetJwtId(accessToken);

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            JwtId = jwtId ?? Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
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

        // Get JWT ID from access token
        var jwtId = _jwtService.GetJwtId(request.AccessToken);
        if (string.IsNullOrEmpty(jwtId))
        {
            throw new UnauthorizedAccessException("Invalid access token");
        }

        // Verify refresh token in database
        var requestHash = HashToken(request.RefreshToken);
        var refreshTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(requestHash);

        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token belongs to the user
        if (refreshTokenEntity.UserId != userId)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token is expired
        if (refreshTokenEntity.IsExpired)
        {
            refreshTokenEntity.IsRevoked = true;
            refreshTokenEntity.RevokedAt = DateTime.UtcNow;
            refreshTokenEntity.ReasonRevoked = "Token expired";
            await _unitOfWork.SaveChangesAsync();
            throw new UnauthorizedAccessException("Refresh token expired");
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
        var newJwtId = _jwtService.GetJwtId(newAccessToken);

        // Mark old refresh token as used
        refreshTokenEntity.UsedAt = DateTime.UtcNow;
        refreshTokenEntity.ReplacedByTokenHash = HashToken(newRefreshToken);

        // Create new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(newRefreshToken),
            JwtId = newJwtId ?? Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

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
        // Find refresh token in database
        var hash = HashToken(refreshToken);
        var refreshTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(hash);
        
        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
        {
            return false;
        }

        // Revoke the specific token
        refreshTokenEntity.IsRevoked = true;
        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
        refreshTokenEntity.ReasonRevoked = "User logout";

        await _unitOfWork.SaveChangesAsync();
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
