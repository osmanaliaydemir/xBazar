namespace Application.DTOs.Payment;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? Last4 { get; set; }
    public string? Brand { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
}

public class CreatePaymentMethodDto
{
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? Last4 { get; set; }
    public string? Brand { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Label { get; set; }
    public bool IsDefault { get; set; } = false;
}

public class UpdatePaymentMethodDto
{
    public string? Label { get; set; }
    public bool? IsDefault { get; set; }
}
