-- =============================================
-- Seed Data
-- =============================================

-- Insert Roles
INSERT INTO [dbo].[Roles] ([Id], [Name], [NormalizedName], [Description], [CreatedAt], [UpdatedAt])
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'Admin', 'ADMIN', 'System Administrator', GETUTCDATE(), GETUTCDATE()),
    ('22222222-2222-2222-2222-222222222222', 'StoreOwner', 'STOREOWNER', 'Store Owner', GETUTCDATE(), GETUTCDATE()),
    ('33333333-3333-3333-3333-333333333333', 'StoreManager', 'STOREMANAGER', 'Store Manager', GETUTCDATE(), GETUTCDATE()),
    ('44444444-4444-4444-4444-444444444444', 'Customer', 'CUSTOMER', 'Customer', GETUTCDATE(), GETUTCDATE());

-- Insert Admin User
INSERT INTO [dbo].[Users] ([Id], [Email], [UserName], [FirstName], [LastName], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [CreatedAt], [UpdatedAt])
VALUES 
    ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'admin@xbazar.com', 'admin', 'Admin', 'User', 1, 
     'AQAAAAEAACcQAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==', -- Password: Admin123!
     'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=', 'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=', GETUTCDATE(), GETUTCDATE());

-- Insert Admin User Role
INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt])
VALUES 
    ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', '11111111-1111-1111-1111-111111111111', GETUTCDATE());

-- Insert Categories
INSERT INTO [dbo].[Categories] ([Id], [Name], [Slug], [Description], [SortOrder], [CreatedAt], [UpdatedAt])
VALUES 
    ('C1111111-1111-1111-1111-111111111111', 'Elektronik', 'elektronik', 'Elektronik ürünler', 1, GETUTCDATE(), GETUTCDATE()),
    ('C2222222-2222-2222-2222-222222222222', 'Giyim', 'giyim', 'Giyim ve aksesuar', 2, GETUTCDATE(), GETUTCDATE()),
    ('C3333333-3333-3333-3333-333333333333', 'Ev & Yaşam', 'ev-yasam', 'Ev ve yaşam ürünleri', 3, GETUTCDATE(), GETUTCDATE()),
    ('C4444444-4444-4444-4444-444444444444', 'Spor & Outdoor', 'spor-outdoor', 'Spor ve outdoor ürünleri', 4, GETUTCDATE(), GETUTCDATE()),
    ('C5555555-5555-5555-5555-555555555555', 'Kitap & Hobi', 'kitap-hobi', 'Kitap ve hobi ürünleri', 5, GETUTCDATE(), GETUTCDATE());

-- Insert Sub Categories
INSERT INTO [dbo].[Categories] ([Id], [Name], [Slug], [Description], [ParentId], [SortOrder], [CreatedAt], [UpdatedAt])
VALUES 
    ('C1111111-1111-1111-1111-111111111112', 'Telefon & Aksesuar', 'telefon-aksesuar', 'Telefon ve aksesuarları', 'C1111111-1111-1111-1111-111111111111', 1, GETUTCDATE(), GETUTCDATE()),
    ('C1111111-1111-1111-1111-111111111113', 'Bilgisayar', 'bilgisayar', 'Bilgisayar ve aksesuarları', 'C1111111-1111-1111-1111-111111111111', 2, GETUTCDATE(), GETUTCDATE()),
    ('C1111111-1111-1111-1111-111111111114', 'TV & Ses', 'tv-ses', 'TV ve ses sistemleri', 'C1111111-1111-1111-1111-111111111111', 3, GETUTCDATE(), GETUTCDATE()),
    ('C2222222-2222-2222-2222-222222222223', 'Erkek Giyim', 'erkek-giyim', 'Erkek giyim ürünleri', 'C2222222-2222-2222-2222-222222222222', 1, GETUTCDATE(), GETUTCDATE()),
    ('C2222222-2222-2222-2222-222222222224', 'Kadın Giyim', 'kadin-giyim', 'Kadın giyim ürünleri', 'C2222222-2222-2222-2222-222222222222', 2, GETUTCDATE(), GETUTCDATE());

-- Insert Sample Store
INSERT INTO [dbo].[Stores] ([Id], [Name], [Slug], [Description], [Email], [PhoneNumber], [IsActive], [IsVerified], [CreatedAt], [UpdatedAt])
VALUES 
    ('S1111111-1111-1111-1111-111111111111', 'TechStore', 'techstore', 'Teknoloji ürünleri mağazası', 'info@techstore.com', '+90 212 555 0123', 1, 1, GETUTCDATE(), GETUTCDATE()),
    ('S2222222-2222-2222-2222-222222222222', 'FashionHub', 'fashionhub', 'Moda ve giyim mağazası', 'info@fashionhub.com', '+90 212 555 0124', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert Store Owner
INSERT INTO [dbo].[Users] ([Id], [Email], [UserName], [FirstName], [LastName], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [CreatedAt], [UpdatedAt])
VALUES 
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 'owner@techstore.com', 'techstore_owner', 'Tech', 'Store Owner', 1, 
     'AQAAAAEAACcQAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==', -- Password: Owner123!
     'BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=', 'BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=', GETUTCDATE(), GETUTCDATE());

-- Insert Store Owner Role
INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt])
VALUES 
    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', '22222222-2222-2222-2222-222222222222', GETUTCDATE());

-- Insert Store User Relationship
INSERT INTO [dbo].[StoreUsers] ([StoreId], [UserId], [Role], [CreatedAt])
VALUES 
    ('S1111111-1111-1111-1111-111111111111', 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB', 'Owner', GETUTCDATE());

-- Insert Sample Products
INSERT INTO [dbo].[Products] ([Id], [StoreId], [CategoryId], [Name], [Slug], [Description], [ShortDescription], [Sku], [Price], [StockQuantity], [IsActive], [CreatedAt], [UpdatedAt])
VALUES 
    ('P1111111-1111-1111-1111-111111111111', 'S1111111-1111-1111-1111-111111111111', 'C1111111-1111-1111-1111-111111111112', 'iPhone 15 Pro', 'iphone-15-pro', 'Apple iPhone 15 Pro 128GB', 'En yeni iPhone modeli', 'IPHONE15PRO128', 45000.00, 10, 1, GETUTCDATE(), GETUTCDATE()),
    ('P2222222-2222-2222-2222-222222222222', 'S1111111-1111-1111-1111-111111111111', 'C1111111-1111-1111-1111-111111111113', 'MacBook Air M2', 'macbook-air-m2', 'Apple MacBook Air 13" M2 256GB', 'Güçlü ve taşınabilir laptop', 'MACBOOKAIRM2', 35000.00, 5, 1, GETUTCDATE(), GETUTCDATE()),
    ('P3333333-3333-3333-3333-333333333333', 'S2222222-2222-2222-2222-222222222222', 'C2222222-2222-2222-2222-222222222223', 'Erkek Polo Tişört', 'erkek-polo-tisort', 'Kaliteli pamuklu polo tişört', 'Rahat ve şık polo tişört', 'POLO001', 150.00, 50, 1, GETUTCDATE(), GETUTCDATE());

-- Insert Sample Coupons
INSERT INTO [dbo].[Coupons] ([Id], [Code], [Name], [Description], [Type], [Value], [MinimumAmount], [UsageLimit], [ValidFrom], [ValidTo], [CreatedAt], [UpdatedAt])
VALUES 
    ('COUPON111-1111-1111-1111-111111111111', 'WELCOME10', 'Hoş Geldin İndirimi', 'Yeni müşteriler için %10 indirim', 'Percentage', 10.00, 100.00, 1000, GETUTCDATE(), DATEADD(MONTH, 3, GETUTCDATE()), GETUTCDATE(), GETUTCDATE()),
    ('COUPON222-2222-2222-2222-222222222222', 'SAVE50', '50 TL İndirim', '200 TL ve üzeri alışverişlerde 50 TL indirim', 'FixedAmount', 50.00, 200.00, 500, GETUTCDATE(), DATEADD(MONTH, 6, GETUTCDATE()), GETUTCDATE(), GETUTCDATE());
