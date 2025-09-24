-- =============================================
-- Product Related Tables
-- =============================================

-- Products Table
CREATE TABLE [dbo].[Products] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [StoreId] UNIQUEIDENTIFIER NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(300) NOT NULL,
    [Slug] NVARCHAR(300) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [ShortDescription] NVARCHAR(500) NULL,
    [Sku] NVARCHAR(100) NOT NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [ComparePrice] DECIMAL(18,2) NULL,
    [CostPrice] DECIMAL(18,2) NULL,
    [StockQuantity] INT NOT NULL DEFAULT 0,
    [MinStockQuantity] INT NOT NULL DEFAULT 0,
    [Weight] DECIMAL(8,2) NULL,
    [Length] DECIMAL(8,2) NULL,
    [Width] DECIMAL(8,2) NULL,
    [Height] DECIMAL(8,2) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDigital] BIT NOT NULL DEFAULT 0,
    [RequiresShipping] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL
);

-- ProductImages Table
CREATE TABLE [dbo].[ProductImages] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [ImageUrl] NVARCHAR(500) NOT NULL,
    [AltText] NVARCHAR(200) NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL
);

-- ProductAttributes Table
CREATE TABLE [dbo].[ProductAttributes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Value] NVARCHAR(500) NOT NULL,
    [SortOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL
);
