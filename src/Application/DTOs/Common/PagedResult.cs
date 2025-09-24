namespace Application.DTOs.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? StatusCode { get; set; }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }
    
    public static ApiResponse<T> Error<T>(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
    
    // Common error helpers
    public static ApiResponse<T> NotFound<T>(string message = "Not found")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = 404
        };
    }
    
    public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized")
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = 401
        };
    }
    
    public static ApiResponse<T> ValidationError<T>(List<string> errors, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message ?? "Validation error",
            Errors = errors,
            StatusCode = 400
        };
    }
    
    public static ApiResponse Error(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}