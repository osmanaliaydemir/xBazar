using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Auth;
using Application.DTOs.Common;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { ex.Message }));
        }
        catch (InvalidOperationException ex)
        {
            return HandleResult(ApiResponse.Error("Invalid operation", new List<string> { ex.Message }));
        }
        catch (Exception)
        {
            return HandleResult(ApiResponse.Error("An error occurred during registration"));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.ExpiresAt.AddDays(7)
                });
            return Ok(new { accessToken = result.AccessToken, expiresAt = result.ExpiresAt, user = result.User });
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleResult(ApiResponse.Unauthorized<object>(ex.Message));
        }
        catch (Exception)
        {
            return HandleResult(ApiResponse.Error("An error occurred during login"));
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Prefer cookie over body
            var refreshToken = Request.Cookies["refresh_token"] ?? request.RefreshToken;
            var req = new RefreshTokenRequest { AccessToken = request.AccessToken, RefreshToken = refreshToken };
            var result = await _authService.RefreshTokenAsync(req);
            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.ExpiresAt.AddDays(7)
                });
            return Ok(new { accessToken = result.AccessToken, expiresAt = result.ExpiresAt, user = result.User });
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleResult(ApiResponse.Unauthorized<object>(ex.Message));
        }
        catch (Exception)
        {
            return HandleResult(ApiResponse.Error("An error occurred during token refresh"));
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var refreshToken = Request.Cookies["refresh_token"] ?? request.RefreshToken;
            var result = await _authService.LogoutAsync(refreshToken);
            Response.Cookies.Delete("refresh_token", new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
            return Ok(new { success = result });
        }
        catch (Exception)
        {
            return HandleResult(ApiResponse.Error("An error occurred during logout"));
        }
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var result = await _authService.ValidateTokenAsync(request.Token);
            return Ok(new { valid = result });
        }
        catch (Exception)
        {
            return HandleResult(ApiResponse.Error("An error occurred during token validation"));
        }
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        var userId = await _authService.GetUserIdByEmailAsync(email);
        if (userId == null)
        {
            return HandleResult(ApiResponse.NotFound<object>("User not found"));
        }
        var ok = await _userService.ConfirmEmailAsync(userId.Value, token);
        return Ok(new { success = ok });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var ok = await _userService.ForgotPasswordAsync(dto.Email);
        return Ok(new { success = ok });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var ok = await _userService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);
        return Ok(new { success = ok });
    }
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ConfirmEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
