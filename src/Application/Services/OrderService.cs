using Application.DTOs.Common;
using Application.DTOs.Order;
using Core.Entities;
using Core.Interfaces;
using Core.Exceptions;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly ICacheService _cache;

    public OrderService(IUnitOfWork uow, IPaymentService paymentService, INotificationService notificationService, ICacheService cache)
    {
        _uow = uow;
        _paymentService = paymentService;
        _notificationService = notificationService;
        _cache = cache;
    }

    public async Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null || order.IsDeleted) throw new NotFoundException("Order not found");
        return ApiResponse.Success(await MapOrderAsync(order));
    }

    public async Task<ApiResponse<OrderDto>> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _uow.Orders.GetAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted);
        if (order == null) throw new NotFoundException("Order not found");
        return ApiResponse.Success(await MapOrderAsync(order));
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetByUserAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var all = await _uow.Orders.GetAllAsync(o => o.UserId == userId && !o.IsDeleted);
        var total = all.Count();
        var items = all
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var dtos = new List<OrderDto>();
        foreach (var o in items) dtos.Add(await MapOrderAsync(o));
        return ApiResponse.Success(new PagedResult<OrderDto> { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize });
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetByStoreAsync(Guid storeId, int page = 1, int pageSize = 20)
    {
        var all = await _uow.Orders.GetAllAsync(o => o.StoreId == storeId && !o.IsDeleted);
        var total = all.Count();
        var items = all.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = new List<OrderDto>();
        foreach (var o in items) dtos.Add(await MapOrderAsync(o));
        return ApiResponse.Success(new PagedResult<OrderDto> { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize });
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var all = await _uow.Orders.GetAllAsync(o => !o.IsDeleted);
        var total = all.Count();
        var items = all.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = new List<OrderDto>();
        foreach (var o in items) dtos.Add(await MapOrderAsync(o));
        return ApiResponse.Success(new PagedResult<OrderDto> { Items = dtos, TotalCount = total, Page = page, PageSize = pageSize });
    }

    public async Task<ApiResponse<OrderDto>> CreateAsync(CreateOrderDto createOrderDto)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = createOrderDto.UserId,
            StoreId = createOrderDto.StoreId,
            Status = OrderStatus.New,
            SubTotal = 0,
            TaxAmount = 0,
            ShippingAmount = 0,
            DiscountAmount = 0,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.Orders.AddAsync(order);
        await _uow.SaveChangesAsync();

        // Map items if provided
        if (createOrderDto.OrderItems?.Any() == true)
        {
            foreach (var item in createOrderDto.OrderItems)
            {
                var product = await _uow.Products.GetByIdAsync(item.ProductId);
                if (product == null || product.IsDeleted) throw new NotFoundException($"Product {item.ProductId} not found");
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSKU = product.SKU,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * item.Quantity,
                    ProductAttributes = item.ProductAttributes
                };
                await _uow.OrderItems.AddAsync(orderItem);
                order.SubTotal += orderItem.TotalPrice;
            }
        }

        // Simple totals (Tax 18%)
        order.TaxAmount = Math.Round(order.SubTotal * 0.18m, 2);
        order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingAmount - order.DiscountAmount;
        order.UpdatedAt = DateTime.UtcNow;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        if (order.UserId.HasValue)
        {
            await _notificationService.NotifyOrderCreatedAsync(order.UserId.Value, order.OrderNumber, order.TotalAmount);
        }

        return ApiResponse.Success(await MapOrderAsync(order));
    }

    public async Task<ApiResponse<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusDto updateOrderStatusDto, Guid userId)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null) return ApiResponse.Error<OrderDto>("Order not found");

        order.Status = updateOrderStatusDto.Status;
        order.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(updateOrderStatusDto.TrackingNumber))
        {
            order.TrackingNumber = updateOrderStatusDto.TrackingNumber;
            order.ShippingCompany = updateOrderStatusDto.ShippingCompany;
        }

        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        if (order.UserId.HasValue)
        {
            await _notificationService.NotifyOrderStatusChangedAsync(order.UserId.Value, order.OrderNumber, order.Status.ToString());
        }

        return ApiResponse.Success(await MapOrderAsync(order));
    }

    public async Task<ApiResponse<bool>> CancelAsync(Guid id, string reason, Guid userId)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null) return ApiResponse.Error<bool>("Order not found");
        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;
        order.UpdatedAt = DateTime.UtcNow;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();
        if (order.UserId.HasValue)
        {
            await _notificationService.NotifyOrderStatusChangedAsync(order.UserId.Value, order.OrderNumber, order.Status.ToString());
        }
        return ApiResponse.Success(true);
    }

    public async Task<ApiResponse<bool>> ProcessPaymentAsync(Guid orderId, PaymentRequestDto paymentRequest)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId);
        if (order == null) return ApiResponse.Error<bool>("Order not found");

        // Idempotency check
        var idempKey = string.IsNullOrWhiteSpace(paymentRequest.IdempotencyKey)
            ? null
            : $"idemp:payment:{orderId}:{paymentRequest.IdempotencyKey}";
        if (idempKey != null)
        {
            var cached = await _cache.GetAsync<ApiResponse<bool>>(idempKey);
            if (cached != null)
            {
                return cached;
            }
        }

        var result = await _paymentService.ProcessPaymentAsync(new PaymentRequest
        {
            OrderId = orderId,
            Amount = order.TotalAmount,
            CardNumber = paymentRequest.CardNumber,
            CardHolderName = paymentRequest.CardHolderName,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Cvv = paymentRequest.Cvv,
            Installment = paymentRequest.Installment,
            CallbackUrl = $"/api/payments/webhook/{paymentRequest.PaymentMethod?.ToLowerInvariant()}"
        });

        if (!result.IsSuccess)
        {
            var error = ApiResponse.Error<bool>(result.ErrorMessage ?? "Payment failed");
            if (idempKey != null)
            {
                await _cache.SetAsync(idempKey, error, TimeSpan.FromMinutes(10));
            }
            return error;
        }

        var payment = new Payment
        {
            OrderId = orderId,
            Amount = order.TotalAmount,
            PaymentMethod = paymentRequest.PaymentMethod,
            PaymentProviderTransactionId = result.TransactionId ?? string.Empty,
            Status = PaymentStatus.Completed,
            ProcessedAt = DateTime.UtcNow
        };
        await _uow.Payments.AddAsync(payment);

        order.PaidAt = DateTime.UtcNow;
        order.Status = OrderStatus.Paid;
        order.UpdatedAt = DateTime.UtcNow;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        if (order.UserId.HasValue)
        {
            await _notificationService.NotifyOrderStatusChangedAsync(order.UserId.Value, order.OrderNumber, order.Status.ToString());
        }

        var success = ApiResponse.Success(true);
        if (idempKey != null)
        {
            await _cache.SetAsync(idempKey, success, TimeSpan.FromMinutes(10));
        }
        return success;
    }

    public async Task<ApiResponse<bool>> ProcessRefundAsync(Guid orderId, RefundRequestDto refundRequest)
    {
        var order = await _uow.Orders.GetByIdAsync(orderId);
        if (order == null) return ApiResponse.Error<bool>("Order not found");

        var lastPayment = (await _uow.Payments.GetAllAsync(p => p.OrderId == orderId))
            .OrderByDescending(p => p.ProcessedAt).FirstOrDefault();
        if (lastPayment == null) return ApiResponse.Error<bool>("No payment to refund");

        var result = await _paymentService.ProcessRefundAsync(new RefundRequest
        {
            TransactionId = lastPayment.PaymentProviderTransactionId,
            Amount = refundRequest.Amount
        });

        if (!result.IsSuccess)
        {
            return ApiResponse.Error<bool>(result.ErrorMessage ?? "Refund failed");
        }

        lastPayment.RefundTransactionId = result.RefundTransactionId;
        lastPayment.RefundedAt = DateTime.UtcNow;
        lastPayment.RefundAmount = refundRequest.Amount;
        await _uow.Payments.UpdateAsync(lastPayment);

        order.Status = OrderStatus.Refunded;
        order.UpdatedAt = DateTime.UtcNow;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        if (order.UserId.HasValue)
        {
            await _notificationService.NotifyOrderStatusChangedAsync(order.UserId.Value, order.OrderNumber, order.Status.ToString());
        }

        return ApiResponse.Success(true);
    }

    public async Task<ApiResponse<OrderDto>> GetGuestOrderAsync(string orderNumber, string email)
    {
        var order = await _uow.Orders.GetAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted);
        if (order == null) return ApiResponse.Error<OrderDto>("Order not found");
        var user = order.UserId.HasValue ? await _uow.Users.GetByIdAsync(order.UserId.Value) : null;
        if (user != null && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse.Error<OrderDto>("Order not found");
        }
        return ApiResponse.Success(await MapOrderAsync(order));
    }

    public async Task<ApiResponse<bool>> UpdateTrackingAsync(Guid id, string trackingNumber, string shippingCompany, Guid userId)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null) return ApiResponse.Error<bool>("Order not found");
        order.TrackingNumber = trackingNumber;
        order.ShippingCompany = shippingCompany;
        order.UpdatedAt = DateTime.UtcNow;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();
        return ApiResponse.Success(true);
    }

    public async Task<ApiResponse<bool>> MarkAsDeliveredAsync(Guid id, Guid userId)
    {
        var order = await _uow.Orders.GetByIdAsync(id);
        if (order == null) return ApiResponse.Error<bool>("Order not found");
        order.Status = OrderStatus.Delivered;
        order.DeliveredAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();
        if (order.UserId.HasValue)
        {
            await _notificationService.NotifyOrderStatusChangedAsync(order.UserId.Value, order.OrderNumber, order.Status.ToString());
        }
        return ApiResponse.Success(true);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }

    private async Task<OrderDto> MapOrderAsync(Order order)
    {
        var store = await _uow.Stores.GetByIdAsync(order.StoreId);
        var items = await _uow.OrderItems.GetAllAsync(oi => oi.OrderId == order.Id);
        var payments = await _uow.Payments.GetAllAsync(p => p.OrderId == order.Id);
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            StoreId = order.StoreId,
            StoreName = store?.Name ?? string.Empty,
            Status = order.Status,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            CouponCode = order.CouponCode,
            Notes = order.Notes,
            PaidAt = order.PaidAt,
            PackedAt = order.PackedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CancelledAt = order.CancelledAt,
            CancellationReason = order.CancellationReason,
            TrackingNumber = order.TrackingNumber,
            ShippingCompany = order.ShippingCompany,
            CreatedAt = order.CreatedAt,
            OrderItems = items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSKU = i.ProductSKU,
                ProductImageUrl = null,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                ProductAttributes = i.ProductAttributes
            }).ToList(),
            Payments = payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentProviderTransactionId = p.PaymentProviderTransactionId,
                Status = p.Status,
                ErrorMessage = p.ErrorMessage,
                ProcessedAt = p.ProcessedAt,
                FailedAt = p.FailedAt,
                RefundTransactionId = p.RefundTransactionId,
                RefundedAt = p.RefundedAt,
                RefundAmount = p.RefundAmount
            }).ToList()
        };
    }
}

