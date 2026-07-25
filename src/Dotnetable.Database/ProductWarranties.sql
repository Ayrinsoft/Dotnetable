CREATE TABLE [dbo].[ProductWarranties] (
    [ProductWarrantyID] INT             IDENTITY (1, 1) NOT NULL,
    [ProductID]         INT             NOT NULL,
    [WarrantyID]        INT             NULL,
    [CustomTitle]       NVARCHAR (200)  NULL,
    [CustomDescription] NVARCHAR (2000) NULL,
    [SortOrder]         INT             CONSTRAINT [DF_ProductWarranties_SortOrder] DEFAULT ((0)) NOT NULL,
    [IsActive]          BIT             CONSTRAINT [DF_ProductWarranties_IsActive] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ProductWarranties] PRIMARY KEY CLUSTERED ([ProductWarrantyID] ASC),
    CONSTRAINT [FK_ProductWarranties_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_ProductWarranties_Warranties] FOREIGN KEY ([WarrantyID]) REFERENCES [dbo].[Warranties] ([WarrantyID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductWarranties_ProductID]
    ON [dbo].[ProductWarranties] ([ProductID] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_ProductWarranties_WarrantyID]
    ON [dbo].[ProductWarranties] ([WarrantyID] ASC);
