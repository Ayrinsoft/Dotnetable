CREATE TABLE [dbo].[PostTags] (
    [PostID] INT NOT NULL,
    [TagID]  INT NOT NULL,
    CONSTRAINT [PK_PostTags] PRIMARY KEY CLUSTERED ([PostID] ASC, [TagID] ASC),
    CONSTRAINT [FK_PostTags_Posts] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Posts] ([PostID]),
    CONSTRAINT [FK_PostTags_Tags] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tags] ([TagID])
);

