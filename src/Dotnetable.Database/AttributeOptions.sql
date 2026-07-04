CREATE TABLE [dbo].[AttributeOption] (
    [AttributeOptionID]     INT             IDENTITY (1, 1) NOT NULL,
    [AttributeDefinitionID] INT             NOT NULL,
    [Value]                 NVARCHAR (2000) NOT NULL,
    [ColorHex]              CHAR (7)        NULL,
    [SortOrder]             INT             CONSTRAINT [DF_AttributeOption_SortOrder] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AttributeOption] PRIMARY KEY CLUSTERED ([AttributeOptionID] ASC),
    CONSTRAINT [FK_AttributeOption_AttributeDefinition] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinition] ([AttributeDefinitionID])
);

