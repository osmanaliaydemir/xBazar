namespace Core.Constants;

public static class Permissions
{
    // User Management
    public const string Users_Read = "users.read";
    public const string Users_Write = "users.write";
    public const string Users_Delete = "users.delete";
    public const string Users_ManageRoles = "users.manage_roles";

    // Store Management
    public const string Stores_Read = "stores.read";
    public const string Stores_Write = "stores.write";
    public const string Stores_Delete = "stores.delete";
    public const string Stores_ManageUsers = "stores.manage_users";

    // Product Management
    public const string Products_Read = "products.read";
    public const string Products_Write = "products.write";
    public const string Products_Delete = "products.delete";
    public const string Products_ManageInventory = "products.manage_inventory";

    // Order Management
    public const string Orders_Read = "orders.read";
    public const string Orders_Write = "orders.write";
    public const string Orders_Delete = "orders.delete";
    public const string Orders_ManageStatus = "orders.manage_status";

    // Payment Management
    public const string Payments_Read = "payments.read";
    public const string Payments_Write = "payments.write";
    public const string Payments_Refund = "payments.refund";

    // Cart Management
    public const string Cart_Read = "cart.read";
    public const string Cart_Write = "cart.write";
    public const string Cart_Checkout = "cart.checkout";

    // Messaging
    public const string Messages_Read = "messages.read";
    public const string Messages_Write = "messages.write";
    public const string Messages_Delete = "messages.delete";

    // Reports
    public const string Reports_Read = "reports.read";
    public const string Reports_Export = "reports.export";

    // System Administration
    public const string System_Admin = "system.admin";
    public const string System_ManageRoles = "system.manage_roles";
    public const string System_ManagePermissions = "system.manage_permissions";
    public const string System_ViewLogs = "system.view_logs";

    // Store-specific permissions
    public const string Store_Owner = "store.owner";
    public const string Store_Manager = "store.manager";
    public const string Store_Support = "store.support";
    public const string Store_Packer = "store.packer";

    // Permission groups for easier management
    public static class Groups
    {
        public static readonly string[] UserManagement = { Users_Read, Users_Write, Users_Delete, Users_ManageRoles };
        public static readonly string[] StoreManagement = { Stores_Read, Stores_Write, Stores_Delete, Stores_ManageUsers };
        public static readonly string[] ProductManagement = { Products_Read, Products_Write, Products_Delete, Products_ManageInventory };
        public static readonly string[] OrderManagement = { Orders_Read, Orders_Write, Orders_Delete, Orders_ManageStatus };
        public static readonly string[] PaymentManagement = { Payments_Read, Payments_Write, Payments_Refund };
        public static readonly string[] CartManagement = { Cart_Read, Cart_Write, Cart_Checkout };
        public static readonly string[] Messaging = { Messages_Read, Messages_Write, Messages_Delete };
        public static readonly string[] Reports = { Reports_Read, Reports_Export };
        public static readonly string[] SystemAdmin = { System_Admin, System_ManageRoles, System_ManagePermissions, System_ViewLogs };
    }
}