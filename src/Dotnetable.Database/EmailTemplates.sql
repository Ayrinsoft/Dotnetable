CREATE TABLE [dbo].[EmailTemplates] (
    [EmailTemplateID] INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]        INT             NOT NULL,
    [TemplateKey]      VARCHAR (64)    NOT NULL,
    [Name]             NVARCHAR (128)  NOT NULL,
    [Subject]          NVARCHAR (256)  NOT NULL,
    [HtmlBody]         NVARCHAR (MAX)  NOT NULL,
    [AccountType]      TINYINT         NOT NULL,
    [Active]           BIT             NOT NULL,
    CONSTRAINT [PK_EmailTemplates] PRIMARY KEY CLUSTERED ([EmailTemplateID] ASC),
    CONSTRAINT [FK_EmailTemplates_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_EmailTemplates_WebsiteID_TemplateKey]
    ON [dbo].[EmailTemplates] ([WebsiteID] ASC, [TemplateKey] ASC);
