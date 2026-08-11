CREATE TABLE [dbo].[Policies] (
    [PolicyID]  INT          IDENTITY (1, 1) NOT NULL,
    [Title]     VARCHAR (64) NOT NULL,
    [Active]    BIT          NOT NULL,
    [WebsiteID] INT          NOT NULL,
    CONSTRAINT [PK_Policies] PRIMARY KEY CLUSTERED ([PolicyID] ASC),
    CONSTRAINT [FK_Policies_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO

CREATE NONCLUSTERED INDEX [IX_Policies_WebsiteID]
    ON [dbo].[Policies] ([WebsiteID] ASC);
