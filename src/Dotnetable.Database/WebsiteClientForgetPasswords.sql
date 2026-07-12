CREATE TABLE [dbo].[WebsiteClientForgetPasswords] (
    [WebsiteClientForgetPasswordID] INT         IDENTITY (1, 1) NOT NULL,
    [ForgetKey]                     VARCHAR (8) NOT NULL,
    [WebsiteClientID]               INT         NOT NULL,
    [LogTime]                       DATETIME    NOT NULL,
    CONSTRAINT [PK_WebsiteClientForgetPasswords] PRIMARY KEY CLUSTERED ([WebsiteClientForgetPasswordID] ASC),
    CONSTRAINT [FK_WebsiteClientForgetPasswords_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteClientForgetPasswords_WebsiteClientID]
    ON [dbo].[WebsiteClientForgetPasswords] ([WebsiteClientID] ASC);
