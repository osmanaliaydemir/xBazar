using Microsoft.EntityFrameworkCore;
using Core.Interfaces;
using Core.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed = false;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new Repository<Core.Entities.User>(_context);
        Roles = new Repository<Core.Entities.Role>(_context);
        UserRoles = new Repository<Core.Entities.UserRole>(_context);
        RolePermissions = new Repository<Core.Entities.RolePermission>(_context);
        ApiKeys = new Repository<Core.Entities.ApiKey>(_context);
        Addresses = new Repository<Core.Entities.Address>(_context);
        Stores = new Repository<Core.Entities.Store>(_context);
        StoreUsers = new Repository<Core.Entities.StoreUser>(_context);
        Categories = new Repository<Core.Entities.Category>(_context);
        Products = new Repository<Core.Entities.Product>(_context);
        ProductImages = new Repository<Core.Entities.ProductImage>(_context);
        ProductAttributes = new Repository<Core.Entities.ProductAttribute>(_context);
        Orders = new Repository<Core.Entities.Order>(_context);
        OrderItems = new Repository<Core.Entities.OrderItem>(_context);
        OrderStatusHistories = new Repository<Core.Entities.OrderStatusHistory>(_context);
        Payments = new Repository<Core.Entities.Payment>(_context);
        PaymentMethods = new Repository<Core.Entities.PaymentMethod>(_context);
        Coupons = new Repository<Core.Entities.Coupon>(_context);
        CouponUsages = new Repository<Core.Entities.CouponUsage>(_context);
        Messages = new Repository<Core.Entities.Message>(_context);
        MessageThreads = new Repository<Core.Entities.MessageThread>(_context);
        Favorites = new Repository<Core.Entities.Favorite>(_context);
        Reviews = new Repository<Core.Entities.Review>(_context);
        AuditLogs = new Repository<Core.Entities.AuditLog>(_context);
        SecurityEvents = new Repository<Core.Entities.SecurityEvent>(_context);
        RefreshTokens = new RefreshTokenRepository(_context);
    }

    public IRepository<Core.Entities.User> Users { get; }
    public IRepository<Core.Entities.Role> Roles { get; }
    public IRepository<Core.Entities.UserRole> UserRoles { get; }
    public IRepository<Core.Entities.RolePermission> RolePermissions { get; }
    public IRepository<Core.Entities.ApiKey> ApiKeys { get; }
    public IRepository<Core.Entities.Address> Addresses { get; }
    public IRepository<Core.Entities.Store> Stores { get; }
    public IRepository<Core.Entities.StoreUser> StoreUsers { get; }
    public IRepository<Core.Entities.Category> Categories { get; }
    public IRepository<Core.Entities.Product> Products { get; }
    public IRepository<Core.Entities.ProductImage> ProductImages { get; }
    public IRepository<Core.Entities.ProductAttribute> ProductAttributes { get; }
    public IRepository<Core.Entities.Order> Orders { get; }
    public IRepository<Core.Entities.OrderItem> OrderItems { get; }
    public IRepository<Core.Entities.OrderStatusHistory> OrderStatusHistories { get; }
    public IRepository<Core.Entities.Payment> Payments { get; }
    public IRepository<Core.Entities.PaymentMethod> PaymentMethods { get; }
    public IRepository<Core.Entities.Coupon> Coupons { get; }
    public IRepository<Core.Entities.CouponUsage> CouponUsages { get; }
    public IRepository<Core.Entities.Message> Messages { get; }
    public IRepository<Core.Entities.MessageThread> MessageThreads { get; }
    public IRepository<Core.Entities.Favorite> Favorites { get; }
    public IRepository<Core.Entities.Review> Reviews { get; }
    public IRepository<Core.Entities.AuditLog> AuditLogs { get; }
    public IRepository<Core.Entities.SecurityEvent> SecurityEvents { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}
