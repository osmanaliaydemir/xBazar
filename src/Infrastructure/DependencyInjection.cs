using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Core.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Authorization;
using StackExchange.Redis;
using Application.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Redis");
            return ConnectionMultiplexer.Connect(connectionString ?? "localhost:6379, abortConnect=false");
        });

        // HttpContext accessor
        services.TryAddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IDistributedLockService, DistributedLockService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IShippingService, ShippingService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISecurityEventService, SecurityEventService>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<ISoftDeleteService, SoftDeleteService>();
        services.AddScoped<ICartService, CartService>();

        // Authorization
        services.AddPermissionAuthorization();

        // HTTP Client for external services
        services.AddHttpClient();

        // Configuration - Remove these for now as the classes don't exist
        // services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
        // services.Configure<PaymentOptions>(configuration.GetSection("Payment"));

        return services;
    }
}
