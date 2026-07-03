CREATE TABLE [dbo].[Menu] (
    [MenuID]    INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID] INT            NOT NULL,
    [Name]      NVARCHAR (100) NOT NULL,
    [Location]  TINYINT        NOT NULL,
    [IsActive]  BIT            CONSTRAINT [DF_Menu_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Menu] PRIMARY KEY CLUSTERED ([MenuID] ASC),
    CONSTRAINT [FK_Menu_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

