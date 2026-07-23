CREATE TABLE [dbo].[ProductTranslations] (
    [ProductTranslationID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductID]            INT             NOT NULL,
    [LanguageCode]         CHAR (2)        NOT NULL,
    [Title]                NVARCHAR (300)  NOT NULL,
    [Slug]                 NVARCHAR (300)  NOT NULL,
    [ShortDescription]     NVARCHAR (1000) NULL,
    [Content]              NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_ProductTranslations] PRIMARY KEY CLUSTERED ([ProductTranslationID] ASC),
    CONSTRAINT [FK_ProductTranslations_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductTranslations_ProductID]
    ON [dbo].[ProductTranslations] ([ProductID] ASC);
