CREATE TABLE [dbo].[PolicyRoles] (
    [PolicyRoleID] INT      IDENTITY (1, 1) NOT NULL,
    [PolicyID]     INT      NOT NULL,
    [RoleID]       SMALLINT NOT NULL,
    [Active]       BIT      NOT NULL,
    CONSTRAINT [PK_PolicyRoles] PRIMARY KEY CLUSTERED ([PolicyRoleID] ASC),
    CONSTRAINT [FK_PolicyRoles_Policies] FOREIGN KEY ([PolicyID]) REFERENCES [dbo].[Policies] ([PolicyID]),
    CONSTRAINT [FK_PolicyRoles_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Roles] ([RoleID])
);

