CREATE TABLE [dbo].[FormFieldOptions] (
    [FormFieldOptionID] INT            IDENTITY (1, 1) NOT NULL,
    [FormFieldID]       INT            NOT NULL,
    [Label]             NVARCHAR (300) NOT NULL,
    [Value]             NVARCHAR (200) NULL,
    [SortOrder]         INT            CONSTRAINT [DF_FormFieldOptions_SortOrder] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_FormFieldOptions] PRIMARY KEY CLUSTERED ([FormFieldOptionID] ASC),
    CONSTRAINT [FK_FormFieldOptions_FormFields] FOREIGN KEY ([FormFieldID]) REFERENCES [dbo].[FormFields] ([FormFieldID])
);
