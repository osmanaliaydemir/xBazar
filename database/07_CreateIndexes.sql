-- =============================================
-- Indexes for Performance
-- =============================================

-- Users Indexes
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]);
CREATE NONCLUSTERED INDEX [IX_Users_UserName] ON [dbo].[Users] ([UserName]);
CREATE NONCLUSTERED INDEX [IX_Users_IsActive] ON [dbo].[Users] ([IsActive]);
CREATE NONCLUSTERED INDEX [IX_Users_IsDeleted] ON [dbo].[Users] ([IsDeleted]);
CREATE NONCLUSTERED INDEX [IX_Users_CreatedAt] ON [dbo].[Users] ([CreatedAt]);

-- Roles Indexes
CREATE NONCLUSTERED INDEX [IX_Roles_Name] ON [dbo].[Roles] ([Name]);
CREATE NONCLUSTERED INDEX [IX_Roles_NormalizedName] ON [dbo].[Roles] ([NormalizedName]);

-- Addresses Indexes
CREATE NONCLUSTERED INDEX [IX_Addresses_UserId] ON [dbo].[Addresses] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_Addresses_IsDefault] ON [dbo].[Addresses] ([IsDefault]);

-- Categories Indexes
CREATE NONCLUSTERED INDEX [IX_Categories_ParentId] ON [dbo].[Categories] ([ParentId]);
CREATE NONCLUSTERED INDEX [IX_Categories_Slug] ON [dbo].[Categories] ([Slug]);
CREATE NONCLUSTERED INDEX [IX_Categories_IsActive] ON [dbo].[Categories] ([IsActive]);
CREATE NONCLUSTERED INDEX [IX_Categories_SortOrder] ON [dbo].[Categories] ([SortOrder]);

-- Stores Indexes
CREATE NONCLUSTERED INDEX [IX_Stores_Slug] ON [dbo].[Stores] ([Slug]);
CREATE NONCLUSTERED INDEX [IX_Stores_Email] ON [dbo].[Stores] ([Email]);
CREATE NONCLUSTERED INDEX [IX_Stores_IsActive] ON [dbo].[Stores] ([IsActive]);
CREATE NONCLUSTERED INDEX [IX_Stores_IsVerified] ON [dbo].[Stores] ([IsVerified]);

-- Products Indexes
CREATE NONCLUSTERED INDEX [IX_Products_StoreId] ON [dbo].[Products] ([StoreId]);
CREATE NONCLUSTERED INDEX [IX_Products_CategoryId] ON [dbo].[Products] ([CategoryId]);
CREATE NONCLUSTERED INDEX [IX_Products_Slug] ON [dbo].[Products] ([Slug]);
CREATE NONCLUSTERED INDEX [IX_Products_Sku] ON [dbo].[Products] ([Sku]);
CREATE NONCLUSTERED INDEX [IX_Products_Price] ON [dbo].[Products] ([Price]);
CREATE NONCLUSTERED INDEX [IX_Products_IsActive] ON [dbo].[Products] ([IsActive]);
CREATE NONCLUSTERED INDEX [IX_Products_IsDeleted] ON [dbo].[Products] ([IsDeleted]);
CREATE NONCLUSTERED INDEX [IX_Products_CreatedAt] ON [dbo].[Products] ([CreatedAt]);

-- ProductImages Indexes
CREATE NONCLUSTERED INDEX [IX_ProductImages_ProductId] ON [dbo].[ProductImages] ([ProductId]);
CREATE NONCLUSTERED INDEX [IX_ProductImages_SortOrder] ON [dbo].[ProductImages] ([SortOrder]);

-- ProductAttributes Indexes
CREATE NONCLUSTERED INDEX [IX_ProductAttributes_ProductId] ON [dbo].[ProductAttributes] ([ProductId]);
CREATE NONCLUSTERED INDEX [IX_ProductAttributes_Name] ON [dbo].[ProductAttributes] ([Name]);

-- Orders Indexes
CREATE NONCLUSTERED INDEX [IX_Orders_OrderNumber] ON [dbo].[Orders] ([OrderNumber]);
CREATE NONCLUSTERED INDEX [IX_Orders_UserId] ON [dbo].[Orders] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_Orders_StoreId] ON [dbo].[Orders] ([StoreId]);
CREATE NONCLUSTERED INDEX [IX_Orders_Status] ON [dbo].[Orders] ([Status]);
CREATE NONCLUSTERED INDEX [IX_Orders_CreatedAt] ON [dbo].[Orders] ([CreatedAt]);

-- OrderItems Indexes
CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId] ON [dbo].[OrderItems] ([OrderId]);
CREATE NONCLUSTERED INDEX [IX_OrderItems_ProductId] ON [dbo].[OrderItems] ([ProductId]);

-- OrderStatusHistory Indexes
CREATE NONCLUSTERED INDEX [IX_OrderStatusHistory_OrderId] ON [dbo].[OrderStatusHistory] ([OrderId]);
CREATE NONCLUSTERED INDEX [IX_OrderStatusHistory_CreatedAt] ON [dbo].[OrderStatusHistory] ([CreatedAt]);

-- Payments Indexes
CREATE NONCLUSTERED INDEX [IX_Payments_OrderId] ON [dbo].[Payments] ([OrderId]);
CREATE NONCLUSTERED INDEX [IX_Payments_Status] ON [dbo].[Payments] ([Status]);
CREATE NONCLUSTERED INDEX [IX_Payments_TransactionId] ON [dbo].[Payments] ([TransactionId]);

-- Coupons Indexes
CREATE NONCLUSTERED INDEX [IX_Coupons_Code] ON [dbo].[Coupons] ([Code]);
CREATE NONCLUSTERED INDEX [IX_Coupons_ValidFrom] ON [dbo].[Coupons] ([ValidFrom]);
CREATE NONCLUSTERED INDEX [IX_Coupons_ValidTo] ON [dbo].[Coupons] ([ValidTo]);
CREATE NONCLUSTERED INDEX [IX_Coupons_IsActive] ON [dbo].[Coupons] ([IsActive]);

-- CouponUsage Indexes
CREATE NONCLUSTERED INDEX [IX_CouponUsage_CouponId] ON [dbo].[CouponUsage] ([CouponId]);
CREATE NONCLUSTERED INDEX [IX_CouponUsage_UserId] ON [dbo].[CouponUsage] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_CouponUsage_OrderId] ON [dbo].[CouponUsage] ([OrderId]);

-- Messages Indexes
CREATE NONCLUSTERED INDEX [IX_Messages_ThreadId] ON [dbo].[Messages] ([ThreadId]);
CREATE NONCLUSTERED INDEX [IX_Messages_SenderId] ON [dbo].[Messages] ([SenderId]);
CREATE NONCLUSTERED INDEX [IX_Messages_ReceiverId] ON [dbo].[Messages] ([ReceiverId]);
CREATE NONCLUSTERED INDEX [IX_Messages_IsRead] ON [dbo].[Messages] ([IsRead]);
CREATE NONCLUSTERED INDEX [IX_Messages_CreatedAt] ON [dbo].[Messages] ([CreatedAt]);

-- Favorites Indexes
CREATE NONCLUSTERED INDEX [IX_Favorites_UserId] ON [dbo].[Favorites] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_Favorites_Type] ON [dbo].[Favorites] ([Type]);
CREATE NONCLUSTERED INDEX [IX_Favorites_ItemId] ON [dbo].[Favorites] ([ItemId]);

-- Reviews Indexes
CREATE NONCLUSTERED INDEX [IX_Reviews_ProductId] ON [dbo].[Reviews] ([ProductId]);
CREATE NONCLUSTERED INDEX [IX_Reviews_UserId] ON [dbo].[Reviews] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_Reviews_Rating] ON [dbo].[Reviews] ([Rating]);
CREATE NONCLUSTERED INDEX [IX_Reviews_IsActive] ON [dbo].[Reviews] ([IsActive]);

-- AuditLogs Indexes
CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityName] ON [dbo].[AuditLogs] ([EntityName]);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_EntityId] ON [dbo].[AuditLogs] ([EntityId]);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_Action] ON [dbo].[AuditLogs] ([Action]);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_CreatedAt] ON [dbo].[AuditLogs] ([CreatedAt]);
CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs] ([UserId]);
