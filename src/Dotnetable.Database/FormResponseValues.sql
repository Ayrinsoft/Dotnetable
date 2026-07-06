CREATE TABLE [dbo].[FormResponseValues] (
    [FormResponseValueID] INT            IDENTITY (1, 1) NOT NULL,
    [FormResponseID]      INT            NOT NULL,
    [FormFieldID]         INT            NOT NULL,
    [Value]               NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_FormResponseValues] PRIMARY KEY CLUSTERED ([FormResponseValueID] ASC),
    CONSTRAINT [FK_FormResponseValues_FormResponses] FOREIGN KEY ([FormResponseID]) REFERENCES [dbo].[FormResponses] ([FormResponseID]),
    CONSTRAINT [FK_FormResponseValues_FormFields] FOREIGN KEY ([FormFieldID]) REFERENCES [dbo].[FormFields] ([FormFieldID])
);
