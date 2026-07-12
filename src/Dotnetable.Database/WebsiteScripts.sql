CREATE TABLE [dbo].[WebsiteScripts] (
    [WebsiteScriptID]     INT             IDENTITY (1, 1) NOT NULL,
    [WebsiteID]           INT             NOT NULL,
    [Name]                VARCHAR (50)    NOT NULL,
    [RawContent]          NVARCHAR (4000) NOT NULL,
    [ScriptPosition]      TINYINT         NOT NULL,
    [ScriptLoadCondition] TINYINT         NOT NULL,
    [LogTime]             DATETIME        NOT NULL,
    [Active]              BIT             NOT NULL,
    [Priority]            TINYINT         NULL,
    CONSTRAINT [PK_WebsiteScripts] PRIMARY KEY CLUSTERED ([WebsiteScriptID] ASC),
    CONSTRAINT [FK_WebsiteScripts_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_WebsiteScripts_WebsiteID]
    ON [dbo].[WebsiteScripts] ([WebsiteID] ASC);
