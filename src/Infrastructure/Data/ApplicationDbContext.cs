using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Core.Entities;
using Core.Interfaces;
using System.Text.Json;
using System.Linq;

namespace Infrastructure.Data;

public partial class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserContext? _currentUser;

    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserContext? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<ApiKey> ApiKeys { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponUsage> CouponUsages { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageThread> MessageThreads { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreUser> StoreUsers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<SecurityEvent> SecurityEvents { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filters for soft delete
        modelBuilder.ApplySoftDeleteFilter<User>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Role>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Address>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Category>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Store>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Product>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Order>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Coupon>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Message>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<MessageThread>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<Review>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<PaymentMethod>(e => !e.IsDeleted);
        modelBuilder.ApplySoftDeleteFilter<SecurityEvent>(e => !e.IsDeleted);

        // Configure relationships
        ConfigureUserRelationships(modelBuilder);
        ConfigureStoreRelationships(modelBuilder);
        ConfigureProductRelationships(modelBuilder);
        ConfigureOrderRelationships(modelBuilder);
        ConfigureMessageRelationships(modelBuilder);
        ConfigureFavoriteRelationships(modelBuilder);
        ConfigurePaymentMethodRelationships(modelBuilder);

        // Disambiguate Address relations
        modelBuilder.Entity<Address>().Ignore(a => a.Orders);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private void ConfigureUserRelationships(ModelBuilder modelBuilder)
    {
        // User -> UserRoles
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Addresses
        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> PaymentMethods
        modelBuilder.Entity<PaymentMethod>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureStoreRelationships(ModelBuilder modelBuilder)
    {
        // Store -> StoreUsers
        modelBuilder.Entity<StoreUser>()
            .HasOne(su => su.Store)
            .WithMany(s => s.StoreUsers)
            .HasForeignKey(su => su.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StoreUser>()
            .HasOne(su => su.User)
            .WithMany(u => u.StoreUsers)
            .HasForeignKey(su => su.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureProductRelationships(ModelBuilder modelBuilder)
    {
        // Product -> Store
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Store)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> Category
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Product -> ProductImages
        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> ProductAttributes
        modelBuilder.Entity<ProductAttribute>()
            .HasOne(pa => pa.Product)
            .WithMany(p => p.ProductAttributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureOrderRelationships(ModelBuilder modelBuilder)
    {
        // Order -> User
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Order -> Store
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Store)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.NoAction);

        // Order -> OrderItems
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> Payment
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureMessageRelationships(ModelBuilder modelBuilder)
    {
        // Message -> MessageThread
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Thread)
            .WithMany(mt => mt.Messages)
            .HasForeignKey(m => m.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message -> User (Sender)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message -> User (Receiver)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureFavoriteRelationships(ModelBuilder modelBuilder)
    {
        // Favorite -> User
        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Favorite -> Product
        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Product)
            .WithMany(p => p.Favorites)
            .HasForeignKey(f => f.ItemId)
            .OnDelete(DeleteBehavior.NoAction);

        // Favorite -> Store
        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Store)
            .WithMany()
            .HasForeignKey(f => f.ItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private void ConfigurePaymentMethodRelationships(ModelBuilder modelBuilder)
    {
        // configured in ConfigureUserRelationships
    }

    private static bool IsSensitiveKey(string key)
    {
        var k = key.ToLowerInvariant();
        return k.Contains("password") || k == "pwd" || k.Contains("token") || k.Contains("secret") || k.EndsWith("key") || k.Contains("cvv") || k.Contains("cardnumber") || k == "card";
    }

    private static object? MaskValue(string key, object? value)
    {
        if (value == null) return null;
        if (!IsSensitiveKey(key)) return value;
        var str = value.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(str)) return "***";
        // For card numbers: keep first 6 and last 4
        var digits = new string(str.Where(char.IsDigit).ToArray());
        if (digits.Length >= 10)
        {
            var masked = digits.Substring(0, Math.Min(6, digits.Length)) + new string('*', Math.Max(0, digits.Length - 10)) + digits.Substring(digits.Length - 4);
            return masked;
        }
        return "***";
    }

    private static Dictionary<string, object?> MaskDictionary(Dictionary<string, object?> dict)
    {
        var result = new Dictionary<string, object?>();
        foreach (var kv in dict)
        {
            result[kv.Key] = MaskValue(kv.Key, kv.Value);
        }
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditLog>();
        var serializerOptions = new JsonSerializerOptions { WriteIndented = false };

        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted)))
        {
            var entity = (BaseEntity)entry.Entity;
            string tableName = entry.Metadata.GetTableName() ?? entity.GetType().Name;
            var audit = new AuditLog
            {
                TableName = tableName ?? "Unknown",
                RecordId = entity.Id,
                Action = entry.State.ToString().ToUpperInvariant(),
                UserId = _currentUser?.UserId?.ToString(),
                UserEmail = _currentUser?.UserEmail,
                IpAddress = _currentUser?.IpAddress,
                UserAgent = _currentUser?.UserAgent,
                Timestamp = DateTime.UtcNow
            };

            if (entry.State == EntityState.Modified)
            {
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey()) continue;
                    if (!prop.IsModified) continue;
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                var maskedOld = MaskDictionary(oldValues);
                var maskedNew = MaskDictionary(newValues);
                audit.OldValues = maskedOld.Count > 0 ? JsonSerializer.Serialize(maskedOld, serializerOptions) : null;
                audit.NewValues = maskedNew.Count > 0 ? JsonSerializer.Serialize(maskedNew, serializerOptions) : null;
            }
            else if (entry.State == EntityState.Added)
            {
                var newValues = entry.Properties.Where(p => !p.Metadata.IsPrimaryKey())
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                var maskedNew = MaskDictionary(newValues);
                audit.OldValues = null;
                audit.NewValues = JsonSerializer.Serialize(maskedNew, serializerOptions);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var oldValues = entry.Properties.Where(p => !p.Metadata.IsPrimaryKey())
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                var maskedOld = MaskDictionary(oldValues);
                audit.OldValues = JsonSerializer.Serialize(maskedOld, serializerOptions);
                audit.NewValues = null;
            }

            auditEntries.Add(audit);
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            await AuditLogs.AddRangeAsync(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
