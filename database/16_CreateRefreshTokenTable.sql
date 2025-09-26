-- =============================================
-- Create RefreshToken Table
-- =============================================

-- RefreshTokens Table
CREATE TABLE [dbo].[RefreshTokens] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [TokenHash] NVARCHAR(500) NOT NULL,
    [JwtId] NVARCHAR(500) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ExpiresAt] DATETIME2 NOT NULL,
    [UsedAt] DATETIME2 NULL,
    [IsRevoked] BIT NOT NULL DEFAULT 0,
    [RevokedAt] DATETIME2 NULL,
    [ReplacedByTokenHash] NVARCHAR(500) NULL,
    [ReasonRevoked] NVARCHAR(500) NULL,
    [IpAddress] NVARCHAR(50) NULL,
    [UserAgent] NVARCHAR(500) NULL
);

-- Create indexes for performance
CREATE INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens] ([UserId]);
CREATE INDEX [IX_RefreshTokens_TokenHash] ON [dbo].[RefreshTokens] ([TokenHash]);
CREATE INDEX [IX_RefreshTokens_JwtId] ON [dbo].[RefreshTokens] ([JwtId]);
CREATE INDEX [IX_RefreshTokens_ExpiresAt] ON [dbo].[RefreshTokens] ([ExpiresAt]);
CREATE INDEX [IX_RefreshTokens_IsRevoked] ON [dbo].[RefreshTokens] ([IsRevoked]);

-- Add foreign key constraint
ALTER TABLE [dbo].[RefreshTokens]
ADD CONSTRAINT [FK_RefreshTokens_Users_UserId] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE;
