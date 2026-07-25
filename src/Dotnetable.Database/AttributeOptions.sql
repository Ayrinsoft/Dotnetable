CREATE TABLE [dbo].[AttributeOptions] (
    [AttributeOptionID]     INT             IDENTITY (1, 1) NOT NULL,
    [AttributeDefinitionID] INT             NOT NULL,
    [Value]                 NVARCHAR (2000) NOT NULL,
    [ColorHex]              CHAR (7)        NULL,
    [SortOrder]             INT NOT NULL,
    CONSTRAINT [PK_AttributeOptions] PRIMARY KEY CLUSTERED ([AttributeOptionID] ASC),
    CONSTRAINT [FK_AttributeOptions_AttributeDefinitions] FOREIGN KEY ([AttributeDefinitionID]) REFERENCES [dbo].[AttributeDefinitions] ([AttributeDefinitionID])
);
GO

CREATE NONCLUSTERED INDEX [IX_AttributeOptions_AttributeDefinitionID]
    ON [dbo].[AttributeOptions] ([AttributeDefinitionID] ASC);
