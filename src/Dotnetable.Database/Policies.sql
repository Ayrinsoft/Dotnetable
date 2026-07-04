CREATE TABLE [dbo].[Policy] (
    [PolicyID]  INT          IDENTITY (1, 1) NOT NULL,
    [Title]     VARCHAR (64) NOT NULL,
    [Active]    BIT          NOT NULL,
    [WebsiteID] INT          NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Policy] PRIMARY KEY CLUSTERED ([PolicyID] ASC),
    CONSTRAINT [FK_Policy_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Policy_WebsiteID]
    ON [dbo].[Policy] ([WebsiteID] ASC);
