CREATE TABLE [dbo].[ProductMedia] (
    [ProductID]  INT NOT NULL,
    [MediaSetID] INT NOT NULL,
    [SortOrder]  INT CONSTRAINT [DF_ProductMedia_SortOrder] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_ProductMedia] PRIMARY KEY CLUSTERED ([ProductID] ASC, [MediaSetID] ASC),
    CONSTRAINT [FK_ProductMedia_MediaSets] FOREIGN KEY ([MediaSetID]) REFERENCES [dbo].[MediaSets] ([MediaSetID]),
    CONSTRAINT [FK_ProductMedia_Products] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Products] ([ProductID])
);
GO

CREATE NONCLUSTERED INDEX [IX_ProductMedia_MediaSetID]
    ON [dbo].[ProductMedia] ([MediaSetID] ASC);
