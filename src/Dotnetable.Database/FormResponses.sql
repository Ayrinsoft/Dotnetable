CREATE TABLE [dbo].[FormResponses] (
    [FormResponseID]  INT          IDENTITY (1, 1) NOT NULL,
    [FormID]          INT          NOT NULL,
    [WebsiteClientID] INT          NULL,
    [SenderIPAddress] VARCHAR (45) NOT NULL,
    [SubmittedAt]     DATETIME     CONSTRAINT [DF_FormResponses_SubmittedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_FormResponses] PRIMARY KEY CLUSTERED ([FormResponseID] ASC),
    CONSTRAINT [FK_FormResponses_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID]),
    CONSTRAINT [FK_FormResponses_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);
GO

CREATE NONCLUSTERED INDEX [IX_FormResponses_FormID]
    ON [dbo].[FormResponses] ([FormID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_FormResponses_WebsiteClientID]
    ON [dbo].[FormResponses] ([WebsiteClientID] ASC);
