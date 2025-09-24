using Core.Entities;

namespace Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<ApiKey> ApiKeys { get; }
    IRepository<Address> Addresses { get; }
    IRepository<Store> Stores { get; }
    IRepository<StoreUser> StoreUsers { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductImage> ProductImages { get; }
    IRepository<ProductAttribute> ProductAttributes { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<OrderStatusHistory> OrderStatusHistories { get; }
    IRepository<Payment> Payments { get; }
    IRepository<PaymentMethod> PaymentMethods { get; }
    IRepository<Coupon> Coupons { get; }
    IRepository<CouponUsage> CouponUsages { get; }
    IRepository<Message> Messages { get; }
    IRepository<MessageThread> MessageThreads { get; }
    IRepository<Favorite> Favorites { get; }
    IRepository<Review> Reviews { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<SecurityEvent> SecurityEvents { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
