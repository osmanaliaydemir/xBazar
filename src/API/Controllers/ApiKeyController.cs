using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Core.Interfaces;
using Application.DTOs.ApiKey;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Require authentication
public class ApiKeyController : BaseController
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpGet]
    [Authorize(Policy = "UsersRead")]
    public async Task<IActionResult> GetApiKeys()
    {
        try
        {
            var userId = GetCurrentUserId();
            var apiKeys = await _apiKeyService.GetUserApiKeysAsync(userId);
            
            var apiKeyDtos = apiKeys.Select(ak => new ApiKeyDto
            {
                Id = ak.Id,
                Name = ak.Name,
                Key = ak.Key,
                Description = ak.Description,
                UserId = ak.UserId,
                IsActive = ak.IsActive,
                ExpiresAt = ak.ExpiresAt,
                LastUsedAt = ak.LastUsedAt,
                UsageCount = ak.UsageCount,
                Environment = ak.Environment,
                CreatedAt = ak.CreatedAt,
                UpdatedAt = ak.UpdatedAt
            }).ToList();

            return Ok(apiKeyDtos);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving API keys" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "UsersRead")]
    public async Task<IActionResult> GetApiKeyById(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var apiKeys = await _apiKeyService.GetUserApiKeysAsync(userId);
            var apiKey = apiKeys.FirstOrDefault(ak => ak.Id == id);
            
            if (apiKey == null)
            {
                return NotFound(new { message = "API key not found" });
            }

            var apiKeyDto = new ApiKeyDto
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                Key = apiKey.Key,
                Description = apiKey.Description,
                UserId = apiKey.UserId,
                IsActive = apiKey.IsActive,
                ExpiresAt = apiKey.ExpiresAt,
                LastUsedAt = apiKey.LastUsedAt,
                UsageCount = apiKey.UsageCount,
                Environment = apiKey.Environment,
                CreatedAt = apiKey.CreatedAt,
                UpdatedAt = apiKey.UpdatedAt
            };

            return Ok(apiKeyDto);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving API key" });
        }
    }

    [HttpPost]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            request.UserId = userId; // Set current user as owner

            var apiKey = await _apiKeyService.CreateApiKeyAsync(
                request.Name,
                request.Description,
                request.UserId,
                request.ExpiresAt,
                request.Environment);

            var apiKeyDto = new ApiKeyDto
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                Key = apiKey.Key,
                Description = apiKey.Description,
                UserId = apiKey.UserId,
                IsActive = apiKey.IsActive,
                ExpiresAt = apiKey.ExpiresAt,
                LastUsedAt = apiKey.LastUsedAt,
                UsageCount = apiKey.UsageCount,
                Environment = apiKey.Environment,
                CreatedAt = apiKey.CreatedAt,
                UpdatedAt = apiKey.UpdatedAt
            };

            return CreatedAtAction(nameof(GetApiKeyById), new { id = apiKey.Id }, apiKeyDto);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating API key" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> UpdateApiKey(Guid id, [FromBody] UpdateApiKeyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var apiKeys = await _apiKeyService.GetUserApiKeysAsync(userId);
            var apiKey = apiKeys.FirstOrDefault(ak => ak.Id == id);
            
            if (apiKey == null)
            {
                return NotFound(new { message = "API key not found" });
            }

            apiKey.Name = request.Name;
            apiKey.Description = request.Description;
            apiKey.IsActive = request.IsActive;
            apiKey.ExpiresAt = request.ExpiresAt;
            apiKey.Environment = request.Environment;
            apiKey.UpdatedAt = DateTime.UtcNow;

            // Note: This would require updating the ApiKeyService to support updates
            // For now, we'll return a not implemented response
            return StatusCode(501, new { message = "Update functionality not yet implemented" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating API key" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "UsersDelete")]
    public async Task<IActionResult> DeleteApiKey(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var apiKeys = await _apiKeyService.GetUserApiKeysAsync(userId);
            var apiKey = apiKeys.FirstOrDefault(ak => ak.Id == id);
            
            if (apiKey == null)
            {
                return NotFound(new { message = "API key not found" });
            }

            var result = await _apiKeyService.DeleteApiKeyAsync(apiKey.Key);
            if (!result)
            {
                return BadRequest(new { message = "Failed to delete API key" });
            }

            return Ok(new { message = "API key deleted successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while deleting API key" });
        }
    }

    [HttpPost("{id}/revoke")]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var apiKeys = await _apiKeyService.GetUserApiKeysAsync(userId);
            var apiKey = apiKeys.FirstOrDefault(ak => ak.Id == id);
            
            if (apiKey == null)
            {
                return NotFound(new { message = "API key not found" });
            }

            var result = await _apiKeyService.RevokeApiKeyAsync(apiKey.Key);
            if (!result)
            {
                return BadRequest(new { message = "Failed to revoke API key" });
            }

            return Ok(new { message = "API key revoked successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while revoking API key" });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateApiKey([FromBody] ValidateApiKeyRequest request)
    {
        try
        {
            var isValid = await _apiKeyService.ValidateApiKeyAsync(request.Key);
            return Ok(new { valid = isValid });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while validating API key" });
        }
    }
}

public class ValidateApiKeyRequest
{
    [Required(ErrorMessage = "API key is required")]
    public string Key { get; set; } = string.Empty;
}
