-- RolePermission tablosu oluşturma
CREATE TABLE [dbo].[RolePermissions] (
    [Id] [uniqueidentifier] NOT NULL DEFAULT (newid()),
    [RoleId] [uniqueidentifier] NOT NULL,
    [Permission] [nvarchar](100) NOT NULL,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    [UpdatedAt] [datetime2](7) NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] [uniqueidentifier] NULL,
    [UpdatedBy] [uniqueidentifier] NULL,
    [IsDeleted] [bit] NOT NULL DEFAULT (0),
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Foreign Key constraint
ALTER TABLE [dbo].[RolePermissions]
ADD CONSTRAINT [FK_RolePermissions_Roles_RoleId] 
FOREIGN KEY([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE;

-- Index'ler
CREATE NONCLUSTERED INDEX [IX_RolePermissions_RoleId] ON [dbo].[RolePermissions] ([RoleId]);
CREATE NONCLUSTERED INDEX [IX_RolePermissions_Permission] ON [dbo].[RolePermissions] ([Permission]);
CREATE NONCLUSTERED INDEX [IX_RolePermissions_IsDeleted] ON [dbo].[RolePermissions] ([IsDeleted]);

-- Unique constraint (RoleId + Permission)
ALTER TABLE [dbo].[RolePermissions]
ADD CONSTRAINT [UK_RolePermissions_RoleId_Permission] UNIQUE ([RoleId], [Permission]);
