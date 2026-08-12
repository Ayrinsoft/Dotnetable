CREATE TABLE [dbo].[StockDocumentLines] (
    [StockDocumentLineID] INT             IDENTITY (1, 1) NOT NULL,
    [StockDocumentID]     INT             NOT NULL,
    [ProductVariantID]    INT             NOT NULL,
    [Quantity]            INT             NOT NULL,
    [BookQuantity]        INT             NOT NULL CONSTRAINT [DF_StockDocumentLines_BookQuantity] DEFAULT ((0)),
    [CountedQuantity]     INT             NULL,
    [UnitCost]            DECIMAL (18, 4) NOT NULL,
    [UnitCostUsd]         DECIMAL (18, 4) NOT NULL,
    [Note]                NVARCHAR (300)  NULL,
    [ReturnCondition]     TINYINT         NOT NULL CONSTRAINT [DF_StockDocumentLines_ReturnCondition] DEFAULT ((0)),
    [HealthGrade]         TINYINT         NOT NULL CONSTRAINT [DF_StockDocumentLines_HealthGrade] DEFAULT ((0)),
    CONSTRAINT [PK_StockDocumentLines] PRIMARY KEY CLUSTERED ([StockDocumentLineID] ASC),
    CONSTRAINT [FK_StockDocumentLines_Docs] FOREIGN KEY ([StockDocumentID]) REFERENCES [dbo].[StockDocuments] ([StockDocumentID]),
    CONSTRAINT [FK_StockDocumentLines_Variants] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID])
);
GO
CREATE NONCLUSTERED INDEX [IX_StockDocumentLines_Doc] ON [dbo].[StockDocumentLines] ([StockDocumentID] ASC);
GO
