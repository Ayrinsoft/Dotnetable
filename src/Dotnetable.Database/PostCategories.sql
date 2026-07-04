CREATE TABLE [dbo].[PostCategory] (
    [PostID]     INT NOT NULL,
    [CategoryID] INT NOT NULL,
    [IsPrimary]  BIT CONSTRAINT [DF_PostCategory_IsPrimary] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_PostCategory] PRIMARY KEY CLUSTERED ([PostID] ASC, [CategoryID] ASC),
    CONSTRAINT [FK_PostCategory_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID]),
    CONSTRAINT [FK_PostCategory_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID])
);

