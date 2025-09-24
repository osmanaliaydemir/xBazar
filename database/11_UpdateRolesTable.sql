-- Roles tablosuna eksik kolonları ekleme
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND name = 'IsSystemRole')
BEGIN
    ALTER TABLE [dbo].[Roles] ADD [IsSystemRole] [bit] NOT NULL DEFAULT (0);
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[Roles] ADD [IsActive] [bit] NOT NULL DEFAULT (1);
END

-- Index'ler
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND name = 'IX_Roles_IsSystemRole')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Roles_IsSystemRole] ON [dbo].[Roles] ([IsSystemRole]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND name = 'IX_Roles_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Roles_IsActive] ON [dbo].[Roles] ([IsActive]);
END
