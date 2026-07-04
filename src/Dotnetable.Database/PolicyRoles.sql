CREATE TABLE [dbo].[PolicyRole] (
    [PolicyRoleID] INT      IDENTITY (1, 1) NOT NULL,
    [PolicyID]     INT      NOT NULL,
    [RoleID]       SMALLINT NOT NULL,
    [Active]       BIT      NOT NULL,
    CONSTRAINT [PK_PolicyRole] PRIMARY KEY CLUSTERED ([PolicyRoleID] ASC),
    CONSTRAINT [FK_PolicyRole_Policy] FOREIGN KEY ([PolicyID]) REFERENCES [dbo].[Policy] ([PolicyID]),
    CONSTRAINT [FK_PolicyRole_Role] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Role] ([RoleID])
);

