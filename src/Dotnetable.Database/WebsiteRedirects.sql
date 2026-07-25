CREATE TABLE [dbo].[WebsiteRedirects] (
    [WebsiteRedirectID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]         INT            NOT NULL,
    [SourcePath]        NVARCHAR (500) NOT NULL,
    [TargetPath]        NVARCHAR (500) NOT NULL,
    [StatusCode]        INT NOT NULL,
    [IsRegex]           BIT NOT NULL,
    [HitCount]          INT NOT NULL,
    [IsActive]          BIT NOT NULL,
    [CreatedAt]         DATETIME NOT NULL,
    CONSTRAINT [PK_WebsiteRedirects] PRIMARY KEY CLUSTERED ([WebsiteRedirectID] ASC),
    CONSTRAINT [FK_WebsiteRedirects_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteRedirects_WebsiteID]
    ON [dbo].[WebsiteRedirects] ([WebsiteID] ASC);
