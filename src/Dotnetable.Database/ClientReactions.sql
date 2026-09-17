CREATE TABLE [dbo].[ClientReactions] (
    [ClientReactionID] INT           IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT           NOT NULL,
    [WebsiteClientID]  INT           NOT NULL,
    [TargetType]       TINYINT       NOT NULL,
    [TargetID]         INT           NOT NULL,
    [ReactionType]     TINYINT       NOT NULL,
    [CreatedAt]        DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_ClientReactions] PRIMARY KEY CLUSTERED ([ClientReactionID] ASC),
    CONSTRAINT [FK_ClientReactions_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ClientReactions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientReactions_Client_Target_Reaction]
    ON [dbo].[ClientReactions] ([WebsiteClientID] ASC, [TargetType] ASC, [TargetID] ASC, [ReactionType] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ClientReactions_WebsiteID_Target]
    ON [dbo].[ClientReactions] ([WebsiteID] ASC, [TargetType] ASC, [TargetID] ASC, [ReactionType] ASC);
