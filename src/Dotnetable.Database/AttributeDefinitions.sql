CREATE TABLE [dbo].[AttributeDefinitions] (
    [AttributeDefinitionID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]             INT            NOT NULL,
    [Name]                  NVARCHAR (200) NOT NULL,
    [Code]                  NVARCHAR (100) NOT NULL,
    [InputType]             TINYINT        NOT NULL,
    [Unit]                  NVARCHAR (30)  NULL,
    [IsFilterable]          BIT            CONSTRAINT [DF_AttributeDefinitions_IsFilterable] DEFAULT ((0)) NOT NULL,
    [IsVariantAttribute]    BIT            CONSTRAINT [DF_AttributeDefinitions_IsVariantAttribute] DEFAULT ((0)) NOT NULL,
    [IsComparable]          BIT            CONSTRAINT [DF_AttributeDefinitions_IsComparable] DEFAULT ((0)) NOT NULL,
    [SortOrder]             INT            CONSTRAINT [DF_AttributeDefinitions_SortOrder] DEFAULT ((0)) NOT NULL,
    [ShowOnTop]             BIT            NOT NULL,
    [Active]                BIT            NOT NULL,
    CONSTRAINT [PK_AttributeDefinitions] PRIMARY KEY CLUSTERED ([AttributeDefinitionID] ASC),
    CONSTRAINT [FK_AttributeDefinitions_Websites] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Websites] ([WebsiteID])
);

