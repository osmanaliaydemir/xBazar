using Application.DTOs.Order;
using Application.DTOs.Common;

namespace Application.Services;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<OrderDto>> GetByOrderNumberAsync(string orderNumber);
    Task<ApiResponse<PagedResult<OrderDto>>> GetByUserAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<ApiResponse<PagedResult<OrderDto>>> GetByStoreAsync(Guid storeId, int page = 1, int pageSize = 20);
    Task<ApiResponse<PagedResult<OrderDto>>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<ApiResponse<OrderDto>> CreateAsync(CreateOrderDto createOrderDto);
    Task<ApiResponse<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusDto updateStatusDto, Guid userId);
    Task<ApiResponse<bool>> CancelAsync(Guid id, string reason, Guid userId);
    Task<ApiResponse<bool>> ProcessPaymentAsync(Guid orderId, PaymentRequestDto paymentRequest);
    Task<ApiResponse<bool>> ProcessRefundAsync(Guid orderId, RefundRequestDto refundRequest);
    Task<ApiResponse<OrderDto>> GetGuestOrderAsync(string orderNumber, string email);
    Task<ApiResponse<bool>> UpdateTrackingAsync(Guid id, string trackingNumber, string shippingCompany, Guid userId);
    Task<ApiResponse<bool>> MarkAsDeliveredAsync(Guid id, Guid userId);
}

public class PaymentRequestDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string? Installment { get; set; }
    public string? IdempotencyKey { get; set; }
}

public class RefundRequestDto
{
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}