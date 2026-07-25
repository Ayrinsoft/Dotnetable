CREATE TABLE [dbo].[AttributeDefinitions] (
    [AttributeDefinitionID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]             INT            NOT NULL,
    [Name]                  NVARCHAR (200) NOT NULL,
    [Code]                  NVARCHAR (100) NOT NULL,
    [InputType]             TINYINT        NOT NULL,
    [Unit]                  NVARCHAR (30)  NULL,
    [IsFilterable]          BIT NOT NULL,
    [IsVariantAttribute]    BIT NOT NULL,
    [IsComparable]          BIT NOT NULL,
    [SortOrder]             INT NOT NULL,
    [ShowOnTop]             BIT            NOT NULL,
    [Active]                BIT            NOT NULL,
    CONSTRAINT [PK_AttributeDefinitions] PRIMARY KEY CLUSTERED ([AttributeDefinitionID] ASC),
    CONSTRAINT [FK_AttributeDefinitions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);
GO

CREATE NONCLUSTERED INDEX [IX_AttributeDefinitions_WebsiteID]
    ON [dbo].[AttributeDefinitions] ([WebsiteID] ASC);
