namespace Core.Constants;

public static class Roles
{
    // System Roles
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Guest = "Guest";

    // Store Roles
    public const string StoreOwner = "StoreOwner";
    public const string StoreManager = "StoreManager";
    public const string StoreSupport = "StoreSupport";
    public const string StorePacker = "StorePacker";

    // Role-Permission mappings
    public static class PermissionMappings
    {
        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            [SuperAdmin] = Permissions.Groups.SystemAdmin.Concat(Permissions.Groups.UserManagement)
                .Concat(Permissions.Groups.StoreManagement)
                .Concat(Permissions.Groups.ProductManagement)
                .Concat(Permissions.Groups.OrderManagement)
                .Concat(Permissions.Groups.PaymentManagement)
                .Concat(Permissions.Groups.CartManagement)
                .Concat(Permissions.Groups.Messaging)
                .Concat(Permissions.Groups.Reports)
                .ToArray(),

            [Admin] = Permissions.Groups.UserManagement
                .Concat(Permissions.Groups.StoreManagement)
                .Concat(Permissions.Groups.ProductManagement)
                .Concat(Permissions.Groups.OrderManagement)
                .Concat(Permissions.Groups.PaymentManagement)
                .Concat(Permissions.Groups.CartManagement)
                .Concat(Permissions.Groups.Messaging)
                .Concat(Permissions.Groups.Reports)
                .ToArray(),

            [User] = new[]
            {
                Permissions.Products_Read,
                Permissions.Stores_Read,
                Permissions.Cart_Read,
                Permissions.Cart_Write,
                Permissions.Cart_Checkout,
                Permissions.Orders_Read,
                Permissions.Messages_Read,
                Permissions.Messages_Write
            },

            [StoreOwner] = Permissions.Groups.StoreManagement
                .Concat(Permissions.Groups.ProductManagement)
                .Concat(Permissions.Groups.OrderManagement)
                .Concat(Permissions.Groups.PaymentManagement)
                .Concat(Permissions.Groups.Messaging)
                .Concat(Permissions.Groups.Reports)
                .ToArray(),

            [StoreManager] = new[]
            {
                Permissions.Stores_Read,
                Permissions.Products_Read,
                Permissions.Products_Write,
                Permissions.Products_ManageInventory,
                Permissions.Orders_Read,
                Permissions.Orders_Write,
                Permissions.Orders_ManageStatus,
                Permissions.Messages_Read,
                Permissions.Messages_Write,
                Permissions.Reports_Read
            },

            [StoreSupport] = new[]
            {
                Permissions.Orders_Read,
                Permissions.Orders_Write,
                Permissions.Messages_Read,
                Permissions.Messages_Write
            },

            [StorePacker] = new[]
            {
                Permissions.Orders_Read,
                Permissions.Orders_Write,
                Permissions.Orders_ManageStatus
            }
        };
    }
}
