using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Services;
using Application.DTOs.Order;
using Application.DTOs.Common;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : BaseController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            if (result.StatusCode == null) result.StatusCode = 404;
        }
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto createOrderDto)
    {
        var result = await _orderService.CreateAsync(createOrderDto);
        return HandleResult(result);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto updateStatusDto)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.UpdateStatusAsync(id, updateStatusDto, userId);
        return HandleResult(result);
    }

    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderDto cancelDto)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.CancelAsync(id, cancelDto.Reason, userId);
        return HandleResult(result);
    }

    [HttpPost("{id}/payment")]
    [Authorize]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] PaymentRequestDto paymentRequest)
    {
        var result = await _orderService.ProcessPaymentAsync(id, paymentRequest);
        return HandleResult(result);
    }

    [HttpPost("{id}/refund")]
    [Authorize]
    public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] RefundRequestDto refundRequest)
    {
        var result = await _orderService.ProcessRefundAsync(id, refundRequest);
        return HandleResult(result);
    }

    [HttpPost("{id}/refund-request")]
    [Authorize]
    public async Task<IActionResult> RequestRefund(Guid id, [FromBody] RefundRequestDto refundRequest)
    {
        var userId = GetCurrentUserId();
        var order = await _orderService.GetByIdAsync(id);
        if (!order.IsSuccess || order.Data == null)
        {
            return HandleResult(ApiResponse.NotFound<object>("Order not found"));
        }
        if (order.Data.UserId == null || order.Data.UserId.Value != userId)
        {
            return Forbid("You can only request refund for your own order.");
        }
        var result = await _orderService.ProcessRefundAsync(id, refundRequest);
        return HandleResult(result);
    }

    [HttpGet("guest/{orderNumber}")]
    public async Task<IActionResult> GetGuestOrder(string orderNumber, [FromQuery] string email)
    {
        var result = await _orderService.GetGuestOrderAsync(orderNumber, email);
        if (!result.IsSuccess)
        {
            if (result.StatusCode == null) result.StatusCode = 404;
        }
        return HandleResult(result);
    }

    [HttpPut("{id}/tracking")]
    [Authorize]
    public async Task<IActionResult> UpdateTracking(Guid id, [FromQuery] string trackingNumber, [FromQuery] string shippingCompany)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.UpdateTrackingAsync(id, trackingNumber, shippingCompany, userId);
        return HandleResult(result);
    }

    [HttpPost("{id}/delivered")]
    [Authorize]
    public async Task<IActionResult> MarkAsDelivered(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.MarkAsDeliveredAsync(id, userId);
        return HandleResult(result);
    }
}

public class CancelOrderDto
{
    public string Reason { get; set; } = string.Empty;
}
