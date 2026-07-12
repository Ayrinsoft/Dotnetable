CREATE TABLE [dbo].[PostCategories] (
    [PostID]     INT NOT NULL,
    [CategoryID] INT NOT NULL,
    [IsPrimary]  BIT CONSTRAINT [DF_PostCategories_IsPrimary] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_PostCategories] PRIMARY KEY CLUSTERED ([PostID] ASC, [CategoryID] ASC),
    CONSTRAINT [FK_PostCategories_Categories] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID]),
    CONSTRAINT [FK_PostCategories_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);
GO

CREATE NONCLUSTERED INDEX [IX_PostCategories_CategoryID]
    ON [dbo].[PostCategories] ([CategoryID] ASC);
