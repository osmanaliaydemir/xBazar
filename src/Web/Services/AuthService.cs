using System.Security.Claims;
using Application.DTOs.Auth;
using Application.DTOs.User;
using Application.DTOs.Common;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Services;

public class AuthService
{
    private readonly ApiService _apiService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(ApiService apiService, AuthenticationStateProvider authStateProvider)
    {
        _apiService = apiService;
        _authStateProvider = authStateProvider;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var response = await _apiService.PostAsync<AuthResponse>("auth/login", request);
        
        if (response.IsSuccess && response.Data != null)
        {
            // Token'ı localStorage'a kaydet
            await SetTokenAsync(response.Data.AccessToken);
            
            // Authentication state'i güncelle
            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                await customProvider.UpdateAuthenticationStateAsync(response.Data.AccessToken);
            }
        }
        
        return response;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var response = await _apiService.PostAsync<AuthResponse>("auth/register", request);
        
        if (response.IsSuccess && response.Data != null)
        {
            // Token'ı localStorage'a kaydet
            await SetTokenAsync(response.Data.AccessToken);
            
            // Authentication state'i güncelle
            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                await customProvider.UpdateAuthenticationStateAsync(response.Data.AccessToken);
            }
        }
        
        return response;
    }

    public async Task LogoutAsync()
    {
        // Token'ı temizle
        await RemoveTokenAsync();
        
        // Authentication state'i temizle
        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
        {
            await customProvider.UpdateAuthenticationStateAsync(null);
        }
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return ApiResponse.Unauthorized<UserDto>("Token bulunamadı");
        }

        _apiService.SetAuthToken(token);
        return await _apiService.GetAsync<UserDto>("auth/me");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    private async Task SetTokenAsync(string token)
    {
        // Bu metod gerçek implementasyonda localStorage veya secure storage kullanacak
        // Şimdilik basit bir implementasyon
        await Task.CompletedTask;
    }

    private async Task<string?> GetTokenAsync()
    {
        // Bu metod gerçek implementasyonda localStorage veya secure storage'dan okuyacak
        // Şimdilik basit bir implementasyon
        await Task.CompletedTask;
        return null;
    }

    private async Task RemoveTokenAsync()
    {
        // Bu metod gerçek implementasyonda localStorage veya secure storage'ı temizleyecek
        await Task.CompletedTask;
    }
}

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public async Task UpdateAuthenticationStateAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else
        {
            // Token'dan claims çıkar (JWT decode)
            var claims = ExtractClaimsFromToken(token);
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        await Task.CompletedTask;
    }

    private static IEnumerable<Claim> ExtractClaimsFromToken(string token)
    {
        // Bu metod gerçek implementasyonda JWT token'ı decode edecek
        // Şimdilik basit bir implementasyon
        return new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, "User")
        };
    }
}
