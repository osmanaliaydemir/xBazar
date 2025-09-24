-- Default rolleri ekleme
INSERT INTO [dbo].[Roles] ([Id], [Name], [Description], [IsSystemRole], [IsActive], [CreatedAt], [UpdatedAt])
VALUES 
    (NEWID(), 'SuperAdmin', 'Sistem yöneticisi - Tüm yetkilere sahip', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Admin', 'Yönetici - Çoğu yetkiye sahip', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'StoreOwner', 'Mağaza sahibi - Mağaza yönetimi', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'StoreManager', 'Mağaza yöneticisi - Mağaza işlemleri', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Customer', 'Müşteri - Temel işlemler', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Role ID'lerini al
DECLARE @SuperAdminId UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[Roles] WHERE [Name] = 'SuperAdmin');
DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[Roles] WHERE [Name] = 'Admin');
DECLARE @StoreOwnerId UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[Roles] WHERE [Name] = 'StoreOwner');
DECLARE @StoreManagerId UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[Roles] WHERE [Name] = 'StoreManager');
DECLARE @CustomerId UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[Roles] WHERE [Name] = 'Customer');

-- SuperAdmin permissions
INSERT INTO [dbo].[RolePermissions] ([RoleId], [Permission], [CreatedAt], [UpdatedAt])
SELECT @SuperAdminId, Permission, GETUTCDATE(), GETUTCDATE()
FROM (VALUES 
    ('users.read'), ('users.write'), ('users.delete'), ('users.manage'),
    ('stores.read'), ('stores.write'), ('stores.delete'), ('stores.manage'),
    ('products.read'), ('products.write'), ('products.delete'), ('products.manage'),
    ('orders.read'), ('orders.write'), ('orders.delete'), ('orders.manage'),
    ('categories.read'), ('categories.write'), ('categories.delete'), ('categories.manage'),
    ('coupons.read'), ('coupons.write'), ('coupons.delete'), ('coupons.manage'),
    ('reviews.read'), ('reviews.write'), ('reviews.delete'), ('reviews.manage'),
    ('messages.read'), ('messages.write'), ('messages.delete'), ('messages.manage'),
    ('admin.panel'), ('system.settings'), ('audit.logs'), ('reports')
) AS Permissions(Permission);

-- Admin permissions
INSERT INTO [dbo].[RolePermissions] ([RoleId], [Permission], [CreatedAt], [UpdatedAt])
SELECT @AdminId, Permission, GETUTCDATE(), GETUTCDATE()
FROM (VALUES 
    ('users.read'), ('users.write'),
    ('stores.read'), ('stores.write'),
    ('products.read'), ('products.write'),
    ('orders.read'), ('orders.write'),
    ('categories.read'), ('categories.write'), ('categories.delete'), ('categories.manage'),
    ('coupons.read'), ('coupons.write'), ('coupons.delete'), ('coupons.manage'),
    ('reviews.read'), ('reviews.write'), ('reviews.delete'), ('reviews.manage'),
    ('messages.read'), ('messages.write'), ('messages.delete'), ('messages.manage'),
    ('admin.panel'), ('reports')
) AS Permissions(Permission);

-- StoreOwner permissions
INSERT INTO [dbo].[RolePermissions] ([RoleId], [Permission], [CreatedAt], [UpdatedAt])
SELECT @StoreOwnerId, Permission, GETUTCDATE(), GETUTCDATE()
FROM (VALUES 
    ('store.owner'), ('store.products'), ('store.orders'), ('store.analytics'),
    ('products.read'), ('products.write'),
    ('orders.read'), ('orders.write'),
    ('categories.read'),
    ('reviews.read'),
    ('messages.read'), ('messages.write')
) AS Permissions(Permission);

-- StoreManager permissions
INSERT INTO [dbo].[RolePermissions] ([RoleId], [Permission], [CreatedAt], [UpdatedAt])
SELECT @StoreManagerId, Permission, GETUTCDATE(), GETUTCDATE()
FROM (VALUES 
    ('store.products'), ('store.orders'), ('store.analytics'),
    ('products.read'), ('products.write'),
    ('orders.read'), ('orders.write'),
    ('categories.read'),
    ('reviews.read'),
    ('messages.read'), ('messages.write')
) AS Permissions(Permission);

-- Customer permissions
INSERT INTO [dbo].[RolePermissions] ([RoleId], [Permission], [CreatedAt], [UpdatedAt])
SELECT @CustomerId, Permission, GETUTCDATE(), GETUTCDATE()
FROM (VALUES 
    ('customer'), ('order.place'), ('orders.own'), ('profile.manage'), ('favorites.manage'),
    ('products.read'),
    ('stores.read'),
    ('categories.read'),
    ('reviews.read'), ('reviews.write'),
    ('messages.read'), ('messages.write')
) AS Permissions(Permission);
