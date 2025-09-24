using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class ValidateTokenRequest
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
}
