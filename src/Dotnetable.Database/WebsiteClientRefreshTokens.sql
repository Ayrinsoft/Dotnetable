CREATE TABLE [dbo].[WebsiteClientRefreshTokens] (
    [WebsiteClientRefreshTokenID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteClientID]             INT           NOT NULL,
    [WebsiteID]                   INT           NOT NULL,
    [TokenHash]                   VARCHAR (64)  NOT NULL,
    [CreatedAt]                   DATETIME2 (0) NOT NULL,
    [ExpiresAt]                   DATETIME2 (0) NOT NULL,
    [RevokedAt]                   DATETIME2 (0) NULL,
    [ReplacedByTokenID]           INT           NULL,
    [CreatedByIp]                 VARCHAR (45)  NULL,
    CONSTRAINT [PK_WebsiteClientRefreshTokens] PRIMARY KEY CLUSTERED ([WebsiteClientRefreshTokenID] ASC),
    CONSTRAINT [FK_WebsiteClientRefreshTokens_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_WebsiteClientRefreshTokens_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


-- Rotating refresh tokens for signed-in customers. Only the SHA-256 hash is stored, so a database
-- read hands over no live sessions. Each exchange revokes the presented row and points
-- ReplacedByTokenID at its successor; presenting an already-revoked token is treated as theft and
-- revokes the whole family.
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_WebsiteClientRefreshTokens_TokenHash]
    ON [dbo].[WebsiteClientRefreshTokens] ([TokenHash] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteClientRefreshTokens_WebsiteClientID]
    ON [dbo].[WebsiteClientRefreshTokens] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteClientRefreshTokens_WebsiteID]
    ON [dbo].[WebsiteClientRefreshTokens] ([WebsiteID] ASC);
