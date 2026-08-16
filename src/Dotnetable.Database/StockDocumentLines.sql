CREATE TABLE [dbo].[StockDocumentLines] (
    [StockDocumentLineID] INT             IDENTITY (1, 1) NOT NULL,
    [StockDocumentID]     INT             NOT NULL,
    [ProductVariantID]    INT             NOT NULL,
    [Quantity]            INT             NOT NULL,
    [BookQuantity]        INT             DEFAULT ((0)) NOT NULL,
    [CountedQuantity]     INT             NULL,
    [UnitCost]            DECIMAL (18, 4) NOT NULL,
    [UnitCostUsd]         DECIMAL (18, 4) NOT NULL,
    [Note]                NVARCHAR (300)  NULL,
    [ReturnCondition]     TINYINT         DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    [HealthGrade]         TINYINT         DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    CONSTRAINT [PK_StockDocumentLines] PRIMARY KEY CLUSTERED ([StockDocumentLineID] ASC),
    CONSTRAINT [FK_StockDocumentLines_ProductVariants_ProductVariantID] FOREIGN KEY ([ProductVariantID]) REFERENCES [dbo].[ProductVariants] ([ProductVariantID]),
    CONSTRAINT [FK_StockDocumentLines_StockDocuments_StockDocumentID] FOREIGN KEY ([StockDocumentID]) REFERENCES [dbo].[StockDocuments] ([StockDocumentID])
);


GO

GO
CREATE NONCLUSTERED INDEX [IX_StockDocumentLines_StockDocumentID]
    ON [dbo].[StockDocumentLines]([StockDocumentID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_StockDocumentLines_ProductVariantID]
    ON [dbo].[StockDocumentLines]([ProductVariantID] ASC);

