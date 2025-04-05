CREATE TABLE [dbo].[TBM_Policy_Role] (
    [PolicyRoleID] INT      IDENTITY (1, 1) NOT NULL,
    [PolicyID]     INT      NOT NULL,
    [RoleID]       SMALLINT NOT NULL,
    [Active]       BIT      NOT NULL,
    CONSTRAINT [PK_TBM_Policy_Role] PRIMARY KEY CLUSTERED ([PolicyRoleID] ASC),
    CONSTRAINT [FK_TBM_Policy_Role_TB_Policy] FOREIGN KEY ([PolicyID]) REFERENCES [dbo].[TB_Policy] ([PolicyID]),
    CONSTRAINT [FK_TBM_Policy_Role_TB_Role] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[TB_Role] ([RoleID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Policy_Role_RoleID]
    ON [dbo].[TBM_Policy_Role]([RoleID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TBM_Policy_Role_PolicyID]
    ON [dbo].[TBM_Policy_Role]([PolicyID] ASC);

