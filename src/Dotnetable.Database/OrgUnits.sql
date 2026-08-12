CREATE TABLE [dbo].[OrgUnits] (
    [OrgUnitID]       INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [ParentOrgUnitID] INT            NULL,
    [Code]            NVARCHAR (20)  NOT NULL,
    [Name]            NVARCHAR (200) NOT NULL,
    [SortOrder]       INT            DEFAULT ((0)) NOT NULL,
    [IsActive]        BIT            DEFAULT (CONVERT([bit],(1))) NOT NULL,
    CONSTRAINT [PK_OrgUnits] PRIMARY KEY CLUSTERED ([OrgUnitID] ASC),
    CONSTRAINT [FK_OrgUnits_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID]),
    CONSTRAINT [FK_OrgUnits_Parent] FOREIGN KEY ([ParentOrgUnitID]) REFERENCES [dbo].[OrgUnits] ([OrgUnitID])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_OrgUnits_Website_Code] ON [dbo].[OrgUnits] ([WebsiteID] ASC, [Code] ASC);
GO
