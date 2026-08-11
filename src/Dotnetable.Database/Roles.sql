CREATE TABLE [dbo].[Roles] (
    [RoleID]      SMALLINT      IDENTITY (1, 1) NOT NULL,
    [RoleKey]     VARCHAR (42)  NOT NULL,
    [Description] VARCHAR (128) NOT NULL,
    [Active]      BIT           NOT NULL,
    [Category]    TINYINT       NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([RoleID] ASC)
);

