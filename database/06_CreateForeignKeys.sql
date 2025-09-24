-- =============================================
-- Foreign Key Constraints
-- =============================================

-- UserRoles Foreign Keys
ALTER TABLE [dbo].[UserRoles]
ADD CONSTRAINT [FK_UserRoles_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[UserRoles]
ADD CONSTRAINT [FK_UserRoles_Roles] 
FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE;

-- Addresses Foreign Keys
ALTER TABLE [dbo].[Addresses]
ADD CONSTRAINT [FK_Addresses_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

-- Categories Foreign Keys
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [FK_Categories_Categories] 
FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Categories]([Id]) ON DELETE NO ACTION;

-- StoreUsers Foreign Keys
ALTER TABLE [dbo].[StoreUsers]
ADD CONSTRAINT [FK_StoreUsers_Stores] 
FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[StoreUsers]
ADD CONSTRAINT [FK_StoreUsers_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

-- Products Foreign Keys
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [FK_Products_Stores] 
FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [FK_Products_Categories] 
FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([Id]) ON DELETE NO ACTION;

-- ProductImages Foreign Keys
ALTER TABLE [dbo].[ProductImages]
ADD CONSTRAINT [FK_ProductImages_Products] 
FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE;

-- ProductAttributes Foreign Keys
ALTER TABLE [dbo].[ProductAttributes]
ADD CONSTRAINT [FK_ProductAttributes_Products] 
FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE;

-- Orders Foreign Keys
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION;

ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Stores] 
FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores]([Id]) ON DELETE NO ACTION;

ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Addresses_Shipping] 
FOREIGN KEY ([ShippingAddressId]) REFERENCES [dbo].[Addresses]([Id]) ON DELETE NO ACTION;

ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Addresses_Billing] 
FOREIGN KEY ([BillingAddressId]) REFERENCES [dbo].[Addresses]([Id]) ON DELETE NO ACTION;

-- OrderItems Foreign Keys
ALTER TABLE [dbo].[OrderItems]
ADD CONSTRAINT [FK_OrderItems_Orders] 
FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[OrderItems]
ADD CONSTRAINT [FK_OrderItems_Products] 
FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE NO ACTION;

-- OrderStatusHistory Foreign Keys
ALTER TABLE [dbo].[OrderStatusHistory]
ADD CONSTRAINT [FK_OrderStatusHistory_Orders] 
FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE;

-- Payments Foreign Keys
ALTER TABLE [dbo].[Payments]
ADD CONSTRAINT [FK_Payments_Orders] 
FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE;

-- CouponUsage Foreign Keys
ALTER TABLE [dbo].[CouponUsage]
ADD CONSTRAINT [FK_CouponUsage_Coupons] 
FOREIGN KEY ([CouponId]) REFERENCES [dbo].[Coupons]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[CouponUsage]
ADD CONSTRAINT [FK_CouponUsage_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[CouponUsage]
ADD CONSTRAINT [FK_CouponUsage_Orders] 
FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE;

-- Messages Foreign Keys
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_MessageThreads] 
FOREIGN KEY ([ThreadId]) REFERENCES [dbo].[MessageThreads]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_Users_Sender] 
FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_Users_Receiver] 
FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

-- Favorites Foreign Keys
ALTER TABLE [dbo].[Favorites]
ADD CONSTRAINT [FK_Favorites_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

-- Reviews Foreign Keys
ALTER TABLE [dbo].[Reviews]
ADD CONSTRAINT [FK_Reviews_Products] 
FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Reviews]
ADD CONSTRAINT [FK_Reviews_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[Reviews]
ADD CONSTRAINT [FK_Reviews_Orders] 
FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE NO ACTION;
