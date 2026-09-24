CREATE TABLE [dbo].[MessageLogs] (
    [MessageLogID]   BIGINT          IDENTITY (1, 1) NOT NULL,
    [WebsiteID]      INT             NOT NULL,
    [Channel]        TINYINT         NOT NULL,
    [Status]         TINYINT         NOT NULL,
    [Recipient]      NVARCHAR (256)  NOT NULL,
    [RecipientName]  NVARCHAR (200)  NULL,
    [RecipientType]  TINYINT         NOT NULL,
    [RecipientID]    INT             NULL,
    [Subject]        NVARCHAR (300)  NULL,
    [Body]           NVARCHAR (MAX)  NULL,
    [IsBodyRedacted] BIT             NOT NULL,
    [Provider]       NVARCHAR (150)  NULL,
    [Source]         VARCHAR (32)    NOT NULL,
    [Error]          NVARCHAR (1000) NULL,
    [SentByMemberID] INT             NULL,
    [SentByName]     NVARCHAR (200)  NULL,
    [CreatedAt]      DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_MessageLogs] PRIMARY KEY CLUSTERED ([MessageLogID] ASC),
    CONSTRAINT [FK_MessageLogs_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


-- Every outgoing email / SMS / WhatsApp / in-app message and its outcome, written by the senders.
-- Channel = MessageChannel, Status = MessageLogStatus, RecipientType = MessageRecipientType.
-- Bodies carrying one-time codes or reset links are stored as NULL with IsBodyRedacted = 1.
GO

CREATE NONCLUSTERED INDEX [IX_MessageLogs_WebsiteID_CreatedAt]
    ON [dbo].[MessageLogs] ([WebsiteID] ASC, [CreatedAt] DESC);
GO

CREATE NONCLUSTERED INDEX [IX_MessageLogs_CreatedAt]
    ON [dbo].[MessageLogs] ([CreatedAt] DESC);
