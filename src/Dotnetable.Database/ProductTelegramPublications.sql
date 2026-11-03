CREATE TABLE [dbo].[ProductTelegramPublications] (
    [ProductID]   INT            NOT NULL,
    [MessageId]   NVARCHAR (40)  NULL,
    [PublishedAt] DATETIME       NOT NULL,
    CONSTRAINT [PK_ProductTelegramPublications] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_ProductTelegramPublications_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Present when the product has been sent to Telegram at least once. Sending again is allowed.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductTelegramPublications';
GO
