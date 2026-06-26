CREATE TABLE [dbo].[Role] (
    [RoleID]      SMALLINT      IDENTITY (1, 1) NOT NULL,
    [RoleKey]     VARCHAR (42)  NOT NULL,
    [Description] VARCHAR (128) NOT NULL,
    [Active]      BIT           NOT NULL,
    [Category]    TINYINT       NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([RoleID] ASC)
);

