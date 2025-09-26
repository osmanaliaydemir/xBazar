using System.Text;
using System.Text.Json;
using Application.DTOs.Common;

namespace Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return result ?? ApiResponse.Error<T>("API yanıtı boş");
            }
            
            return ApiResponse.Error<T>($"API Hatası: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error<T>($"Bağlantı hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return result ?? ApiResponse.Error<T>("API yanıtı boş");
            }
            
            return ApiResponse.Error<T>($"API Hatası: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error<T>($"Bağlantı hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);
                return result ?? ApiResponse.Error<T>("API yanıtı boş");
            }
            
            return ApiResponse.Error<T>($"API Hatası: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error<T>($"Bağlantı hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return result ?? ApiResponse.Error<T>("API yanıtı boş");
            }
            
            return ApiResponse.Error<T>($"API Hatası: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse.Error<T>($"Bağlantı hatası: {ex.Message}");
        }
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
