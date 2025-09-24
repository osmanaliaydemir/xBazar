using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Core.Constants;

namespace Infrastructure.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<IAuthorizationHandler, StoreResourceHandler>();

        services.AddAuthorization(options =>
        {
            // User Management Policies
            options.AddPolicy("Users_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users_Read)));
            options.AddPolicy("Users_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users_Write)));
            options.AddPolicy("Users_Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users_Delete)));
            options.AddPolicy("Users_ManageRoles", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Users_ManageRoles)));

            // Store Management Policies
            options.AddPolicy("Stores_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Stores_Read)));
            options.AddPolicy("Stores_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Stores_Write)));
            options.AddPolicy("Stores_Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Stores_Delete)));
            options.AddPolicy("Stores_ManageUsers", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Stores_ManageUsers)));

            // Product Management Policies
            options.AddPolicy("Products_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Products_Read)));
            options.AddPolicy("Products_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Products_Write)));
            options.AddPolicy("Products_Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Products_Delete)));
            options.AddPolicy("Products_ManageInventory", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Products_ManageInventory)));

            // Order Management Policies
            options.AddPolicy("Orders_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Orders_Read)));
            options.AddPolicy("Orders_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Orders_Write)));
            options.AddPolicy("Orders_Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Orders_Delete)));
            options.AddPolicy("Orders_ManageStatus", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Orders_ManageStatus)));

            // Payment Management Policies
            options.AddPolicy("Payments_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Payments_Read)));
            options.AddPolicy("Payments_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Payments_Write)));
            options.AddPolicy("Payments_Refund", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Payments_Refund)));

            // Cart Management Policies
            options.AddPolicy("Cart_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Cart_Read)));
            options.AddPolicy("Cart_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Cart_Write)));
            options.AddPolicy("Cart_Checkout", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Cart_Checkout)));

            // Messaging Policies
            options.AddPolicy("Messages_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Messages_Read)));
            options.AddPolicy("Messages_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Messages_Write)));
            options.AddPolicy("Messages_Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Messages_Delete)));

            // Reports Policies
            options.AddPolicy("Reports_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Reports_Read)));
            options.AddPolicy("Reports_Export", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Reports_Export)));

            // System Administration Policies
            options.AddPolicy("System_Admin", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.System_Admin)));
            options.AddPolicy("System_ManageRoles", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.System_ManageRoles)));
            options.AddPolicy("System_ManagePermissions", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.System_ManagePermissions)));
            options.AddPolicy("System_ViewLogs", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.System_ViewLogs)));

            // Store-specific policies (with resource parameter)
            options.AddPolicy("Store_Products_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Products_Read, "store")));
            options.AddPolicy("Store_Products_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Products_Write, "store")));
            options.AddPolicy("Store_Orders_Read", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Orders_Read, "store")));
            options.AddPolicy("Store_Orders_Write", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.Orders_Write, "store")));
        });

        return services;
    }
}
