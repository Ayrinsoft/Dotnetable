CREATE TABLE [dbo].[ProductXPublications] (
    [ProductID]   INT            NOT NULL,
    [TweetId]     NVARCHAR (40)  NULL,
    [PublishedAt] DATETIME       NOT NULL,
    CONSTRAINT [PK_ProductXPublications] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_ProductXPublications_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Present when the product has been sent to X at least once. Sending again is allowed.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ProductXPublications';
GO
