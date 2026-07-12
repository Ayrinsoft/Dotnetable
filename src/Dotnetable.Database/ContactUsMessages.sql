CREATE TABLE [dbo].[ContactUsMessages] (
    [ContactUsMessagesID] INT             IDENTITY (1, 1) NOT NULL,
    [SenderName]          NVARCHAR (64)   NOT NULL,
    [EmailAddress]        VARCHAR (64)    NOT NULL,
    [CellphoneNumber]     VARCHAR (15)    NOT NULL,
    [MessageSubject]      NVARCHAR (512)  NOT NULL,
    [MessageBody]         NVARCHAR (4000) NOT NULL,
    [Archive]             BIT             NOT NULL,
    [LogTime]             DATETIME        NOT NULL,
    [SenderIPAddress]     VARCHAR (15)    NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    CONSTRAINT [PK_ContactUsMessages] PRIMARY KEY CLUSTERED ([ContactUsMessagesID] ASC),
    CONSTRAINT [FK_ContactUsMessages_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ContactUsMessages_WebsiteID]
    ON [dbo].[ContactUsMessages] ([WebsiteID] ASC);
