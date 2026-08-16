CREATE TABLE [dbo].[OrgUnits] (
    [OrgUnitID]       INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]       INT            NOT NULL,
    [ParentOrgUnitID] INT            NULL,
    [Code]            NVARCHAR (20)  NOT NULL,
    [Name]            NVARCHAR (200) NOT NULL,
    [SortOrder]       INT            NOT NULL,
    [IsActive]        BIT            NOT NULL,
    CONSTRAINT [PK_OrgUnits] PRIMARY KEY CLUSTERED ([OrgUnitID] ASC),
    CONSTRAINT [FK_OrgUnits_OrgUnits_ParentOrgUnitID] FOREIGN KEY ([ParentOrgUnitID]) REFERENCES [dbo].[OrgUnits] ([OrgUnitID]),
    CONSTRAINT [FK_OrgUnits_Websites_WebsiteID] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_OrgUnits_Website_Code] ON [dbo].[OrgUnits] ([WebsiteID] ASC, [Code] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_OrgUnits_ParentOrgUnitID]
    ON [dbo].[OrgUnits]([ParentOrgUnitID] ASC);

