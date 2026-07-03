CREATE TABLE [dbo].[AttributeDefinition] (
    [AttributeDefinitionID] INT            IDENTITY (1, 1) NOT NULL,
    [WebsiteID]             INT            NOT NULL,
    [Name]                  NVARCHAR (200) NOT NULL,
    [Code]                  NVARCHAR (100) NOT NULL,
    [InputType]             TINYINT        NOT NULL,
    [Unit]                  NVARCHAR (30)  NULL,
    [IsFilterable]          BIT            CONSTRAINT [DF_AttributeDefinition_IsFilterable] DEFAULT ((0)) NOT NULL,
    [IsVariantAttribute]    BIT            CONSTRAINT [DF_AttributeDefinition_IsVariantAttribute] DEFAULT ((0)) NOT NULL,
    [IsComparable]          BIT            CONSTRAINT [DF_AttributeDefinition_IsComparable] DEFAULT ((0)) NOT NULL,
    [SortOrder]             INT            CONSTRAINT [DF_AttributeDefinition_SortOrder] DEFAULT ((0)) NOT NULL,
    [ShowOnTop]             BIT            NOT NULL,
    [Active]                BIT            NOT NULL,
    CONSTRAINT [PK_AttributeDefinition] PRIMARY KEY CLUSTERED ([AttributeDefinitionID] ASC),
    CONSTRAINT [FK_AttributeDefinition_Website] FOREIGN KEY ([WebsiteID]) REFERENCES [dbo].[Website] ([WebsiteID])
);

