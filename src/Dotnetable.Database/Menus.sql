CREATE TABLE [dbo].[Menus] (
    [MenuID]    INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID] INT            NOT NULL,
    [Name]      NVARCHAR (100) NOT NULL,
    [Location]  TINYINT        NOT NULL,
    [IsActive]  BIT            CONSTRAINT [DF_Menus_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Menus] PRIMARY KEY CLUSTERED ([MenuID] ASC),
    CONSTRAINT [FK_Menus_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Menus_WebsiteID]
    ON [dbo].[Menus] ([WebsiteID] ASC);
