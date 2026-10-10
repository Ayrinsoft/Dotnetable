CREATE TABLE [dbo].[SiteReports] (
    [SiteReportID]    INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT             NOT NULL,
    [WebsiteClientID] INT             NULL,
    [ReporterName]    NVARCHAR (64)   NOT NULL,
    [EmailAddress]    VARCHAR (64)    NOT NULL,
    [Kind]            TINYINT         NOT NULL,
    [Subject]         NVARCHAR (200)  NOT NULL,
    [MessageBody]     NVARCHAR (4000) NOT NULL,
    [PageUrl]         NVARCHAR (500)  NULL,
    [SenderIPAddress] VARCHAR (45)    NOT NULL,
    [Status]          TINYINT         CONSTRAINT [DF_SiteReports_Status] DEFAULT ((0)) NOT NULL,
    [AdminNote]       NVARCHAR (2000) NULL,
    [LogTime]         DATETIME        NOT NULL,
    CONSTRAINT [PK_SiteReports] PRIMARY KEY CLUSTERED ([SiteReportID] ASC),
    CONSTRAINT [FK_SiteReports_WebsiteClients] FOREIGN KEY ([WebsiteClientID]) REFERENCES [dbo].[WebsiteClients] ([WebsiteClientID]),
    CONSTRAINT [FK_SiteReports_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_SiteReports_WebsiteID]
    ON [dbo].[SiteReports] ([WebsiteID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SiteReports_WebsiteClientID]
    ON [dbo].[SiteReports] ([WebsiteClientID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_SiteReports_WebsiteID_Status]
    ON [dbo].[SiteReports] ([WebsiteID] ASC, [Status] ASC);
