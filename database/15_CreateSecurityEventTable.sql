-- SecurityEvents Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SecurityEvents]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SecurityEvents] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [EventType] NVARCHAR(100) NOT NULL,
        [Subject] NVARCHAR(200) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(1000) NULL,
        [Details] NVARCHAR(MAX) NULL,
        [OccurredAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] UNIQUEIDENTIFIER NULL,
        [UpdatedBy] UNIQUEIDENTIFIER NULL
    );
END

-- Indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SecurityEvents_OccurredAt')
BEGIN
    CREATE INDEX [IX_SecurityEvents_OccurredAt] ON [dbo].[SecurityEvents] ([OccurredAt]);
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SecurityEvents_EventType')
BEGIN
    CREATE INDEX [IX_SecurityEvents_EventType] ON [dbo].[SecurityEvents] ([EventType]);
END
