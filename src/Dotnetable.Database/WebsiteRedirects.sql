CREATE TABLE [dbo].[WebsiteRedirects] (
    [WebsiteRedirectID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT            NOT NULL,
    [SourcePath]        NVARCHAR (500) NOT NULL,
    [TargetPath]        NVARCHAR (500) NOT NULL,
    [StatusCode]        INT            CONSTRAINT [DF_WebsiteRedirects_StatusCode] DEFAULT ((301)) NOT NULL,
    [IsRegex]           BIT            CONSTRAINT [DF_WebsiteRedirects_IsRegex] DEFAULT ((0)) NOT NULL,
    [HitCount]          INT            CONSTRAINT [DF_WebsiteRedirects_HitCount] DEFAULT ((0)) NOT NULL,
    [IsActive]          BIT            CONSTRAINT [DF_WebsiteRedirects_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedAt]         DATETIME       CONSTRAINT [DF_WebsiteRedirects_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_WebsiteRedirects] PRIMARY KEY CLUSTERED ([WebsiteRedirectID] ASC),
    CONSTRAINT [FK_WebsiteRedirects_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteRedirects_WebsiteID]
    ON [dbo].[WebsiteRedirects] ([WebsiteID] ASC);
