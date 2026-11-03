CREATE TABLE [dbo].[ProductInstagramPublications] (
    [ProductID]   INT            NOT NULL,
    [IgMediaId]   NVARCHAR (64)  NOT NULL,
    [Permalink]   NVARCHAR (300) NULL,
    [PublishedAt] DATETIME       NOT NULL,
    CONSTRAINT [PK_ProductInstagramPublications] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_ProductInstagramPublications_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Present when the product has been sent to Instagram at least once. Sending again is allowed.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductInstagramPublications';
GO
