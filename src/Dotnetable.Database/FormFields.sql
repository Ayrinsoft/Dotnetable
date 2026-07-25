CREATE TABLE [dbo].[FormFields] (
    [FormFieldID] INT            IDENTITY (1, 1) NOT NULL,
    [FormID]      INT            NOT NULL,
    [Label]       NVARCHAR (300) NOT NULL,
    [FieldType]   TINYINT NOT NULL,
    [Placeholder] NVARCHAR (200) NULL,
    [HelpText]    NVARCHAR (500) NULL,
    [IsRequired]  BIT NOT NULL,
    [MinValue]    INT            NULL,
    [MaxValue]    INT            NULL,
    [SortOrder]   INT NOT NULL,
    [IsActive]    BIT NOT NULL,
    CONSTRAINT [PK_FormFields] PRIMARY KEY CLUSTERED ([FormFieldID] ASC),
    CONSTRAINT [FK_FormFields_Forms] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Forms] ([FormID])
);
GO

CREATE NONCLUSTERED INDEX [IX_FormFields_FormID]
    ON [dbo].[FormFields] ([FormID] ASC);
