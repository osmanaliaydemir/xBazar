-- ApiKey tablosu oluşturma
CREATE TABLE [dbo].[ApiKeys] (
    [Id] [uniqueidentifier] NOT NULL DEFAULT (newid()),
    [Name] [nvarchar](100) NOT NULL,
    [Key] [nvarchar](64) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [UserId] [uniqueidentifier] NULL,
    [IsActive] [bit] NOT NULL DEFAULT (1),
    [ExpiresAt] [datetime2](7) NULL,
    [LastUsedAt] [datetime2](7) NULL,
    [UsageCount] [int] NOT NULL DEFAULT (0),
    [Environment] [nvarchar](50) NULL,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    [UpdatedAt] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] [uniqueidentifier] NULL,
    [UpdatedBy] [uniqueidentifier] NULL,
    [IsDeleted] [bit] NOT NULL DEFAULT (0),
    CONSTRAINT [PK_ApiKeys] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Foreign Key constraint
ALTER TABLE [dbo].[ApiKeys]
ADD CONSTRAINT [FK_ApiKeys_Users_UserId] 
FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE SET NULL;

-- Index'ler
CREATE NONCLUSTERED INDEX [IX_ApiKeys_Key] ON [dbo].[ApiKeys] ([Key]);
CREATE NONCLUSTERED INDEX [IX_ApiKeys_UserId] ON [dbo].[ApiKeys] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_ApiKeys_IsActive] ON [dbo].[ApiKeys] ([IsActive]);
CREATE NONCLUSTERED INDEX [IX_ApiKeys_IsDeleted] ON [dbo].[ApiKeys] ([IsDeleted]);
CREATE NONCLUSTERED INDEX [IX_ApiKeys_Environment] ON [dbo].[ApiKeys] ([Environment]);

-- Unique constraint (Key)
ALTER TABLE [dbo].[ApiKeys]
ADD CONSTRAINT [UK_ApiKeys_Key] UNIQUE ([Key]);
