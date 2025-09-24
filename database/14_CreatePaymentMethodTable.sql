-- PaymentMethods Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentMethods]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PaymentMethods] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [Provider] NVARCHAR(100) NOT NULL,
        [Token] NVARCHAR(200) NOT NULL,
        [Last4] NVARCHAR(4) NULL,
        [Brand] NVARCHAR(50) NULL,
        [ExpiryMonth] NVARCHAR(2) NULL,
        [ExpiryYear] NVARCHAR(4) NULL,
        [Label] NVARCHAR(100) NULL,
        [IsDefault] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] UNIQUEIDENTIFIER NULL,
        [UpdatedBy] UNIQUEIDENTIFIER NULL
    );
END

-- Indexes (idempotent)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentMethods_UserId')
BEGIN
    CREATE INDEX IX_PaymentMethods_UserId ON [dbo].[PaymentMethods] ([UserId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PaymentMethods_UserToken')
BEGIN
    CREATE UNIQUE INDEX UX_PaymentMethods_UserToken ON [dbo].[PaymentMethods] ([UserId], [Provider], [Token]) WHERE [IsDeleted] = 0;
END

-- Foreign Keys (idempotent)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PaymentMethods_Users_UserId')
BEGIN
    ALTER TABLE [dbo].[PaymentMethods]
    ADD CONSTRAINT [FK_PaymentMethods_Users_UserId]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE;
END

-- Single default per user (filtered unique index)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PaymentMethods_User_Default')
BEGIN
    CREATE UNIQUE INDEX [UX_PaymentMethods_User_Default]
    ON [dbo].[PaymentMethods]([UserId])
    WHERE [IsDeleted] = 0 AND [IsDefault] = 1;
END
