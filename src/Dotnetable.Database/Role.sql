CREATE TABLE [dbo].[Role] (
    [RoleID]      SMALLINT      IDENTITY (1, 1) NOT NULL,
    [RoleKey]     VARCHAR (42)  NOT NULL,
    [Description] VARCHAR (128) NOT NULL,
    [Active]      BIT           NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([RoleID] ASC)
);

